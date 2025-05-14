using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;

public static class ScreenCaptureHelper
{
    [DllImport("user32.dll")]
    static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

    [DllImport("gdi32.dll")]
    static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

    [DllImport("gdi32.dll")]
    static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight,
        IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

    [DllImport("gdi32.dll")]
    static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll")]
    static extern bool DeleteDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    static extern int GetDIBits(IntPtr hdc, IntPtr hbmp, uint uStartScan, uint cScanLines, IntPtr lpvBits, ref BITMAPINFO lpbmi, uint uUsage);

    const int SRCCOPY = 0x00CC0020;
    const int BI_RGB = 0;

    [StructLayout(LayoutKind.Sequential)]
    struct BITMAPINFOHEADER
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct BITMAPINFO
    {
        public BITMAPINFOHEADER bmiHeader;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public uint[] bmiColors;
    }

    public static bool CaptureTaskbarArea(string savePath, int width = 64, int height = 64)
    {
        var screen = Screen.PrimaryScreen.Bounds;
        int x = screen.Right - 30 - 140;
        int y = screen.Bottom - 30 - 10;
        return CaptureArea(savePath, x, y, width, height);
    }

    public static Bitmap CaptureTaskbarBitmap(int width = 64, int height = 64)
    {
        var screen = Screen.PrimaryScreen.Bounds;
        int x = screen.Right - 30 - 140;
        int y = screen.Bottom - 30 - 10;
        Bitmap bmp = CaptureBitmap(x, y, width, height);
        if (bmp == null)
        {
            Console.WriteLine($"[CaptureTaskbarBitmap] 截图失败，返回 null。x={x}, y={y}, width={width}, height={height}");
        }
        return bmp;
    }

    public static Bitmap CaptureTaskbarBitmapByFile(int width = 64, int height = 64)
    {
        var screen = Screen.PrimaryScreen.Bounds;
        int x = screen.Right - 30 - 140;
        int y = screen.Bottom - 30 - 10;
        string tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp_taskbar.bmp");
        if (CaptureArea(tempPath, x, y, width, height))
        {
            try
            {
                return new Bitmap(tempPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CaptureTaskbarBitmapByFile] Bitmap加载异常: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("[CaptureTaskbarBitmapByFile] 截图文件生成失败。");
        }
        return null;
    }

    public static bool CaptureArea(string savePath, int x, int y, int width, int height)
    {
        try
        {
            IntPtr hDesk = GetDC(IntPtr.Zero);
            IntPtr hSrcDC = CreateCompatibleDC(hDesk);
            IntPtr hBmp = CreateCompatibleBitmap(hDesk, width, height);
            IntPtr hOld = SelectObject(hSrcDC, hBmp);

            BitBlt(hSrcDC, 0, 0, width, height, hDesk, x, y, SRCCOPY);

            // 获取位图数据
            BITMAPINFO bmi = new BITMAPINFO();
            bmi.bmiHeader.biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER));
            bmi.bmiHeader.biWidth = width;
            bmi.bmiHeader.biHeight = -height; // top-down
            bmi.bmiHeader.biPlanes = 1;
            bmi.bmiHeader.biBitCount = 32;
            bmi.bmiHeader.biCompression = BI_RGB;
            bmi.bmiHeader.biSizeImage = (uint)(width * height * 4);

            int bmpSize = width * height * 4;
            IntPtr pixels = Marshal.AllocHGlobal(bmpSize);
            GetDIBits(hSrcDC, hBmp, 0, (uint)height, pixels, ref bmi, 0);

            // 写入 BMP 文件头
            using (var fs = new FileStream(savePath, FileMode.Create, FileAccess.Write))
            using (var bw = new BinaryWriter(fs))
            {
                int fileHeaderSize = 14;
                int infoHeaderSize = Marshal.SizeOf(typeof(BITMAPINFOHEADER));
                int fileSize = fileHeaderSize + infoHeaderSize + bmpSize;

                // BITMAPFILEHEADER
                bw.Write((ushort)0x4D42); // 'BM'
                bw.Write(fileSize);
                bw.Write((ushort)0);
                bw.Write((ushort)0);
                bw.Write(fileHeaderSize + infoHeaderSize);

                // BITMAPINFOHEADER
                bw.Write(bmi.bmiHeader.biSize);
                bw.Write(bmi.bmiHeader.biWidth);
                bw.Write(bmi.bmiHeader.biHeight);
                bw.Write(bmi.bmiHeader.biPlanes);
                bw.Write(bmi.bmiHeader.biBitCount);
                bw.Write(bmi.bmiHeader.biCompression);
                bw.Write(bmi.bmiHeader.biSizeImage);
                bw.Write(bmi.bmiHeader.biXPelsPerMeter);
                bw.Write(bmi.bmiHeader.biYPelsPerMeter);
                bw.Write(bmi.bmiHeader.biClrUsed);
                bw.Write(bmi.bmiHeader.biClrImportant);

                // 像素数据
                byte[] pixelData = new byte[bmpSize];
                Marshal.Copy(pixels, pixelData, 0, bmpSize);
                bw.Write(pixelData);
            }

            Marshal.FreeHGlobal(pixels);

            SelectObject(hSrcDC, hOld);
            DeleteObject(hBmp);
            DeleteDC(hSrcDC);
            ReleaseDC(IntPtr.Zero, hDesk);

            return File.Exists(savePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Win32纯API截图异常: " + ex.Message);
            return false;
        }
    }

    public static Bitmap CaptureBitmap(int x, int y, int width, int height)
    {
        try
        {
            IntPtr hDesk = GetDC(IntPtr.Zero);
            IntPtr hSrcDC = CreateCompatibleDC(hDesk);
            IntPtr hBmp = CreateCompatibleBitmap(hDesk, width, height);
            IntPtr hOld = SelectObject(hSrcDC, hBmp);

            BitBlt(hSrcDC, 0, 0, width, height, hDesk, x, y, SRCCOPY);

            Bitmap bmp = null;
            try
            {
                bmp = Image.FromHbitmap(hBmp);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CaptureBitmap] Image.FromHbitmap异常: {ex.Message}");
            }

            SelectObject(hSrcDC, hOld);
            DeleteObject(hBmp);
            DeleteDC(hSrcDC);
            ReleaseDC(IntPtr.Zero, hDesk);

            return bmp;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Win32纯API截图异常: " + ex.Message);
            return null;
        }
    }
}
