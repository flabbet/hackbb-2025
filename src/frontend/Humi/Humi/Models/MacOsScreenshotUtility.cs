using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Humi.Models;

public partial class MacOsScreenshotUtility : IScreenshotUtility
{
    [LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static partial IntPtr CGDisplayCreateImage(uint displayId);

    [LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static partial void CGImageRelease(IntPtr image);

    [LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static partial nint CGImageGetWidth(IntPtr image);

    [LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static partial nint CGImageGetHeight(IntPtr image);

    [LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static partial IntPtr CGImageGetDataProvider(IntPtr image);

    [LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static partial IntPtr CGDataProviderCopyData(IntPtr provider);

    [LibraryImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static partial IntPtr CFDataGetBytePtr(IntPtr data);

    [LibraryImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static partial nint CFDataGetLength(IntPtr data);

    [LibraryImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static partial void CFRelease(IntPtr cf);


    public static Bitmap CaptureDisplay(uint displayId)
    {
        IntPtr cgImage = CGDisplayCreateImage(displayId);
        if (cgImage == IntPtr.Zero)
            throw new InvalidOperationException("Failed to capture display image.");

        try
        {
            int width = (int)CGImageGetWidth(cgImage);
            int height = (int)CGImageGetHeight(cgImage);
            IntPtr provider = CGImageGetDataProvider(cgImage);
            IntPtr data = CGDataProviderCopyData(provider);
            try
            {
                IntPtr ptr = CFDataGetBytePtr(data);
                int length = (int)CFDataGetLength(data);

                // CGImage is typically BGRA on macOS
                var bmp = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Premul);
                using var bmpData = bmp.Lock();

                unsafe
                {
                    Buffer.MemoryCopy(
                        (void*)ptr,
                        (void*)bmpData.Address,
                        length,
                        length);
                }

                return bmp;
            }
            finally
            {
                CFRelease(data);
            }
        }
        finally
        {
            CGImageRelease(cgImage);
        }
    }

    public Bitmap CaptureScreen(int screenId)
    {
        return CaptureDisplay((uint)screenId);
    }
}