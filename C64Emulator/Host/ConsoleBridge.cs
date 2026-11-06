using System;
using System.Runtime.InteropServices;

namespace C64Emulator.Host;

/// <summary>
/// The application is built as a Windows GUI executable, so it has no console
/// of its own. When it is started from a terminal with command-line arguments,
/// attach to that terminal and point the standard handles at it so --help and
/// run summaries are visible. A normal double-click attaches to nothing.
/// </summary>
internal static class ConsoleBridge
{
    private const int StdOutputHandle = -11;
    private const int StdErrorHandle = -12;
    private const uint GenericRead = 0x80000000;
    private const uint GenericWrite = 0x40000000;
    private const uint FileShareRead = 0x00000001;
    private const uint FileShareWrite = 0x00000002;
    private const uint OpenExisting = 3;
    private const int AttachParentProcess = -1;

    private static bool attempted;

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AttachConsole(int processId);

    [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateFile(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetStdHandle(int stdHandle, IntPtr handle);

    public static void AttachToParent()
    {
        if (attempted || !OperatingSystem.IsWindows())
            return;

        attempted = true;

        if (!AttachConsole(AttachParentProcess))
            return;

        Redirect(StdOutputHandle);
        Redirect(StdErrorHandle);
    }

    private static void Redirect(int stdHandle)
    {
        IntPtr handle = CreateFile(
            "CONOUT$",
            GenericRead | GenericWrite,
            FileShareRead | FileShareWrite,
            IntPtr.Zero,
            OpenExisting,
            0,
            IntPtr.Zero);

        if (handle != IntPtr.Zero && handle != new IntPtr(-1))
            SetStdHandle(stdHandle, handle);
    }
}
