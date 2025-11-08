using System.Runtime.InteropServices;

namespace InputAutoSwitch
{
    /// <summary>
    /// 输入法管理类
    /// </summary>
    public static class InputMethodManager
    {
        private const int IME_CMODE_NATIVE = 0x0001;
        private const int IME_CMODE_CHINESE = 0x0001;

        /// <summary>
        /// 检测当前输入法是否为中文模式
        /// </summary>
        public static bool IsChineseInputMode()
        {
            try
            {
                IntPtr hwnd = GetForegroundWindow();
                if (hwnd == IntPtr.Zero)
                    return false;

                // 获取默认输入法上下文（如果窗口没有输入上下文）
                IntPtr hIMC = ImmGetContext(hwnd);
                
                // 如果窗口没有输入法上下文，尝试获取默认输入法上下文
                if (hIMC == IntPtr.Zero)
                {
                    hIMC = ImmGetDefaultIMEWnd(hwnd);
                    if (hIMC == IntPtr.Zero)
                        return false;
                    
                    // 对于默认输入法窗口，发送消息查询状态
                    IntPtr result = SendMessage(hIMC, WM_IME_CONTROL, new IntPtr(IMC_GETOPENSTATUS), IntPtr.Zero);
                    return result.ToInt32() != 0;
                }

                try
                {
                    int conversionMode = 0;
                    int sentenceMode = 0;

                    bool success = ImmGetConversionStatus(hIMC, ref conversionMode, ref sentenceMode);
                    
                    // 检查输入法是否打开
                    bool isOpen = ImmGetOpenStatus(hIMC);
                    
                    if (isOpen)
                    {
                        // 如果输入法打开了，就认为是中文模式
                        return true;
                    }
                    
                    // 备用检测：检查 conversionMode
                    if (success && conversionMode > 0)
                    {
                        return true;
                    }
                }
                finally
                {
                    ImmReleaseContext(hwnd, hIMC);
                }
            }
            catch (Exception)
            {
                // 忽略异常,返回false
            }

            return false;
        }
        
        /// <summary>
        /// 获取输入法详细信息（用于调试）
        /// </summary>
        public static string GetInputMethodInfo()
        {
            try
            {
                IntPtr hwnd = GetForegroundWindow();
                if (hwnd == IntPtr.Zero)
                    return "未获取到前台窗口";

                IntPtr hIMC = ImmGetContext(hwnd);
                if (hIMC == IntPtr.Zero)
                {
                    // 尝试获取默认输入法窗口
                    IntPtr hIMEWnd = ImmGetDefaultIMEWnd(hwnd);
                    if (hIMEWnd == IntPtr.Zero)
                        return "未获取到输入法上下文(包括默认窗口)";
                    
                    IntPtr result = SendMessage(hIMEWnd, WM_IME_CONTROL, new IntPtr(IMC_GETOPENSTATUS), IntPtr.Zero);
                    return $"默认IME窗口, OpenStatus={result.ToInt32()}";
                }

                try
                {
                    int conversionMode = 0;
                    int sentenceMode = 0;
                    bool success = ImmGetConversionStatus(hIMC, ref conversionMode, ref sentenceMode);
                    bool isOpen = ImmGetOpenStatus(hIMC);
                    
                    return $"Open={isOpen}, Mode=0x{conversionMode:X}, Sentence=0x{sentenceMode:X}";
                }
                finally
                {
                    ImmReleaseContext(hwnd, hIMC);
                }
            }
            catch (Exception ex)
            {
                return $"异常: {ex.Message}";
            }
        }

        /// <summary>
        /// 模拟按下 Ctrl+Space 组合键
        /// </summary>
        public static void SendCtrlSpace()
        {
            // 方法1: 使用 keybd_event (更兼容)
            keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
            System.Threading.Thread.Sleep(10);
            keybd_event(VK_SPACE, 0, 0, UIntPtr.Zero);
            System.Threading.Thread.Sleep(10);
            keybd_event(VK_SPACE, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            System.Threading.Thread.Sleep(10);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        // Win32 API 常量
        private const byte VK_CONTROL = 0x11;
        private const byte VK_SPACE = 0x20;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const int WM_IME_CONTROL = 0x0283;
        private const int IMC_GETOPENSTATUS = 0x0005;

        // Win32 API 声明
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("imm32.dll")]
        private static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("imm32.dll")]
        private static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);

        [DllImport("imm32.dll")]
        private static extern bool ImmGetConversionStatus(IntPtr hIMC, ref int lpfdwConversion, ref int lpfdwSentence);

        [DllImport("imm32.dll")]
        private static extern bool ImmGetOpenStatus(IntPtr hIMC);

        [DllImport("imm32.dll")]
        private static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
    }
}
