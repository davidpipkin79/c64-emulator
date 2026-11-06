namespace C64Emulator.Host;

/// <summary>
/// Process exit codes for headless runs. They are stable so scripts can tell
/// a usage mistake from missing ROMs, unreadable media or a changed screen.
/// </summary>
internal static class ExitCodes
{
    public const int Success = 0;
    public const int RuntimeFailure = 1;
    public const int Usage = 2;
    public const int MediaFailure = 3;
    public const int RomFailure = 4;
    public const int BootTimeout = 5;
    public const int FrameMismatch = 6;
    public const int ReferenceFailure = 7;
}
