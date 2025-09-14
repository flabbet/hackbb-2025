using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Media.Imaging;

namespace Humi.Models
{
    public partial class LinuxScreenshotUtility : IScreenshotUtility
    {
        private const int ZPixmap = 2;

        [DllImport("libX11.so.6")]
        private static extern IntPtr XOpenDisplay(IntPtr display);

        [DllImport("libX11.so.6")]
        private static extern IntPtr XRootWindow(IntPtr display, int screen);

        [DllImport("libX11.so.6")]
        private static extern IntPtr XGetImage(
            IntPtr display,
            IntPtr window,
            int x,
            int y,
            uint width,
            uint height,
            ulong planeMask,
            int format);

        [DllImport("libX11.so.6")]
        private static extern void XDestroyImage(IntPtr ximage);

        [DllImport("libX11.so.6")]
        private static extern int XDisplayWidth(IntPtr display, int screenNumber);

        [DllImport("libX11.so.6")]
        private static extern int XDisplayHeight(IntPtr display, int screenNumber);

        [StructLayout(LayoutKind.Sequential)]
        private struct XImage
        {
            public int width;
            public int height;
            public int xoffset;
            public int format;
            public IntPtr data;
            public int byte_order;
            public int bitmap_unit;
            public int bitmap_bit_order;
            public int bitmap_pad;
            public int depth;
            public int bytes_per_line;
            public int bits_per_pixel;
            public ulong red_mask;
            public ulong green_mask;
            public ulong blue_mask;
            public IntPtr obdata;
            public IntPtr f; // function pointers omitted
        }

        public static Bitmap CaptureDisplay(int screenId)
        {
            IntPtr display = XOpenDisplay(new IntPtr(0));
            if (display == IntPtr.Zero)
                throw new InvalidOperationException("Cannot open X display.");

            int width = XDisplayWidth(display, 0);
            int height = XDisplayHeight(display, 0);
            IntPtr root = XRootWindow(display, 0);
            
            Console.WriteLine(width + "x" + height);

            IntPtr ximagePtr = XGetImage(display, root, 0, 0, (uint)width, (uint)height, ulong.MaxValue, ZPixmap);
            if (ximagePtr == IntPtr.Zero)
                throw new InvalidOperationException("Failed to get XImage.");

            try
            {
                XImage ximage = Marshal.PtrToStructure<XImage>(ximagePtr);
                var bmp = new WriteableBitmap(
                    new PixelSize(width, height),
                    new Vector(96, 96),
                    Avalonia.Platform.PixelFormat.Bgra8888,
                    Avalonia.Platform.AlphaFormat.Premul);

                using var bmpData = bmp.Lock();
                unsafe
                {
                    Buffer.MemoryCopy(
                        (void*)ximage.data,
                        (void*)bmpData.Address,
                        width * height * 4, // assuming 32bpp
                        width * height * 4);
                }

                return bmp;
            }
            finally
            {
                XDestroyImage(ximagePtr);
            }
        }
        
        private static (int width, int height) GetMonitorSize(int screenId, out int offsetX, out int offsetY)
        {
            offsetX = 0;
            offsetY = 0;
            string output = RunBashCommand("xrandr --listmonitors");

            // Example line: "0: +*eDP-1 1920/344x1080/193+0+0  eDP-1"
            var lines = output.Split('\n');
            if (screenId + 1 >= lines.Length)
                throw new ArgumentOutOfRangeException(nameof(screenId), "ScreenId exceeds available monitors.");

            string line = lines[screenId + 1].Trim();
            var match = Regex.Match(line, @"\d+:\s+[\+\*]*\S+\s+(\d+)/\d+x(\d+)/\d+\+(\d+)\+(\d+)");
            if (!match.Success)
                throw new InvalidOperationException("Failed to parse xrandr output.");

            int width = int.Parse(match.Groups[1].Value);
            int height = int.Parse(match.Groups[2].Value);
            offsetX = int.Parse(match.Groups[3].Value);
            offsetY = int.Parse(match.Groups[4].Value);

            return (width, height);
        }
        
        private static string RunBashCommand(string command)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{command}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output.Trim();
        }

        private static Bitmap CaptureMonitor(int x, int y, int width, int height)
        {
            IntPtr display = XOpenDisplay(IntPtr.Zero);
            if (display == IntPtr.Zero)
                throw new InvalidOperationException("Cannot open X display.");

            IntPtr root = XRootWindow(display, 0);
            IntPtr ximagePtr = XGetImage(display, root, x, y, (uint)width, (uint)height, ulong.MaxValue, ZPixmap);
            if (ximagePtr == IntPtr.Zero)
                throw new InvalidOperationException("Failed to get XImage.");

            try
            {
                XImage ximage = Marshal.PtrToStructure<XImage>(ximagePtr);
                var bmp = new WriteableBitmap(
                    new PixelSize(width, height),
                    new Vector(96, 96),
                    Avalonia.Platform.PixelFormat.Bgra8888,
                    Avalonia.Platform.AlphaFormat.Premul);

                using var bmpData = bmp.Lock();
                unsafe
                {
                    Buffer.MemoryCopy(
                        (void*)ximage.data,
                        (void*)bmpData.Address,
                        width * height * 4,
                        width * height * 4);
                }

                return bmp;
            }
            finally
            {
                XDestroyImage(ximagePtr);
            }
        }

        public Bitmap CaptureScreen(int screenId)
        {
            var monitorSize = GetMonitorSize(screenId, out int offsetX, out int offsetY);
            return CaptureMonitor(offsetX, offsetY, monitorSize.width, monitorSize.height);
            // return CaptureDisplay(screenId);
        }
    }
}
