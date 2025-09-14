using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Platform;

namespace Humi.Utility;

public static class MacOSTransparencyHelper
{
    [DllImport("/usr/lib/libobjc.A.dylib")]
    static extern IntPtr objc_getClass(string name);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    static extern IntPtr sel_registerName(string name);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    static extern void objc_msgSend_void_bool(IntPtr receiver, IntPtr selector, [MarshalAs(UnmanagedType.I1)] bool value);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    static extern IntPtr objc_msgSend_retIntPtr(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    static extern void objc_msgSend_void_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg);
 

    public static void MakeAvaloniaWindowTransparent(Window window)
    {
        // Get the native NSWindow handle from Avalonia
        var handle = window.TryGetPlatformHandle();
        
        if (handle is { HandleDescriptor: "NSWindow" })
        {
            IntPtr nsWindow = handle.Handle;
            IntPtr setOpaqueSel = sel_registerName("setOpaque:");
            objc_msgSend_void_bool(nsWindow, setOpaqueSel, false);

            IntPtr nsColorClass = objc_getClass("NSColor");
            IntPtr clearColorSel = sel_registerName("clearColor");
            IntPtr clearColor = objc_msgSend_retIntPtr(nsColorClass, clearColorSel);

            IntPtr setBackgroundColorSel = sel_registerName("setBackgroundColor:");
            objc_msgSend_void_IntPtr(nsWindow, setBackgroundColorSel, clearColor);
        }
    }
}