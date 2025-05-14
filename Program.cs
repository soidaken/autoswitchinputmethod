using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using PaddleOCRSharp;

partial class Program
{
    static DateTime lastKeyTime = DateTime.Now;
    static System.Threading.Timer timer;
    static IntPtr hookId = IntPtr.Zero;
    static LowLevelKeyboardProc proc = HookCallback;
    static bool hasInputSinceLastSwitch = true;
    static int idleSeconds = 2;
    static PaddleOCREngine ocrEngine = null;

    // Win32 API declarations
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll")]
    static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    [DllImport("user32.dll")]
    static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

    [StructLayout(LayoutKind.Sequential)]
    struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public UIntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public int pt_x;
        public int pt_y;
    }

    const int WH_KEYBOARD_LL = 13;
    const int WM_KEYDOWN = 0x0100;
    const int WM_SYSKEYDOWN = 0x0104;
    const int KEYEVENTF_KEYDOWN = 0x0000;
    const int KEYEVENTF_KEYUP = 0x0002;
    const byte VK_CONTROL = 0x11;
    const byte VK_SPACE = 0x20;

    static void Main(string[] args)
    {
        Console.WriteLine("程序启动，正在初始化...");
        // 解析命令行参数
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals("-time", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(args[i + 1], out int sec) && sec > 0)
                {
                    idleSeconds = sec;
                }
            }
        }
        Console.WriteLine($"空闲秒数设置为: {idleSeconds}");

        // 初始化 OCR 引擎
        string modelDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "inference");
        var config = new OCRModelConfig()
        {
            det_infer = System.IO.Path.Combine(modelDir, "ch_PP-OCRv4_det_infer"),
            rec_infer = System.IO.Path.Combine(modelDir, "ch_PP-OCRv4_rec_infer"),
            cls_infer = System.IO.Path.Combine(modelDir, "ch_ppocr_mobile_v2.0_cls_infer"),
            keys = System.IO.Path.Combine(modelDir, "ppocr_keys.txt")
        };
        var param = new OCRParameter();
        ocrEngine = new PaddleOCREngine(config, param);

        // 安装全局键盘钩子
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            hookId = SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
        Console.WriteLine("全局键盘钩子已安装。");

        timer = new System.Threading.Timer(CheckIdle, null, 0, 1000);

        try
        {
            // 使用标准消息循环，避免高CPU占用
            MSG msg;
            Console.WriteLine("进入消息循环，等待键盘输入...");
            while (GetMessage(out msg, IntPtr.Zero, 0, 0)) { }
        }
        finally
        {
            UnhookWindowsHookEx(hookId);
            Console.WriteLine("程序退出，钩子已卸载。");
        }
    }

    static void CheckIdle(object state)
    {
        if (hasInputSinceLastSwitch && (DateTime.Now - lastKeyTime).TotalSeconds > idleSeconds)
        {
            Console.WriteLine($"检测到空闲超过 {idleSeconds} 秒，检查输入法...");
            if (IsChineseInput())
            {
                Console.WriteLine("当前为中文输入法，模拟 Ctrl+Space 切换。");
                SimulateCtrlSpace();
            }
            else
            {
                Console.WriteLine("当前不是中文输入法，无需切换。");
            }
            hasInputSinceLastSwitch = false;
        }
    }

    static bool IsChineseInput()
    {
        // 只通过截图检测右下角是否有“中”字
        if (DetectChineseIconOnTaskbar())
        {
            Console.WriteLine("屏幕右下角检测到“中”字，判定为中文输入模式。");
            return true;
        }
        else
        {
            Console.WriteLine("屏幕右下角未检测到“中”字，判定为非中文输入模式。");
            return false;
        }
    }

    static bool DetectChineseIconOnTaskbar()
    {
        try
        {
            var t0 = DateTime.Now;
            Console.WriteLine($"[{t0:HH:mm:ss.fff}] 开始截图...");
            string savePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp_taskbar.bmp");
            var t1 = DateTime.Now;
            bool ok = ScreenCaptureHelper.CaptureArea(savePath, Screen.PrimaryScreen.Bounds.Right - 30 - 140, Screen.PrimaryScreen.Bounds.Bottom - 30 - 10, 30, 30);
            var t2 = DateTime.Now;
            if (!ok)
            {
                Console.WriteLine($"[{t2:HH:mm:ss.fff}] 截图失败，未生成文件。");
                return false;
            }
            Console.WriteLine($"[{t2:HH:mm:ss.fff}] 截图完成。截图耗时: {(t2 - t1).TotalMilliseconds} ms");
            if (ocrEngine != null)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] 开始OCR识别...");
                var ocrStart = DateTime.Now;
                var result = ocrEngine.DetectText(savePath);
                var ocrEnd = DateTime.Now;
                string text = result.Text;
                Console.WriteLine($"[{ocrEnd:HH:mm:ss.fff}] OCR识别结果: {text}");
                Console.WriteLine($"OCR耗时: {(ocrEnd - ocrStart).TotalMilliseconds} ms");
                Console.WriteLine($"总耗时: {(ocrEnd - t0).TotalMilliseconds} ms");
                if (text.Contains("中"))
                {
                    Console.WriteLine("OCR检测：有“中”字。");
                    return true;
                }
                else
                {
                    Console.WriteLine("OCR检测：未发现“中”字。");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("DetectChineseIconOnTaskbar异常: " + ex.Message);
        }
        return false;
    }

    static void SimulateCtrlSpace()
    {
        // 按下Ctrl
        keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
        // 按下Space
        keybd_event(VK_SPACE, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
        // 松开Space
        keybd_event(VK_SPACE, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        // 松开Ctrl
        keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        Console.WriteLine("已模拟 Ctrl+Space。");
    }

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
        {
            lastKeyTime = DateTime.Now;
            hasInputSinceLastSwitch = true;
            Console.WriteLine($"检测到键盘输入，重置计时。时间: {lastKeyTime:T}");
        }
        return CallNextHookEx(hookId, nCode, wParam, lParam);
    }
}
