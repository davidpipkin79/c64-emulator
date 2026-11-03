namespace C64Emulator
{
    /// <summary>
    /// The CBM DOS file types used in both D64 directory entries and T64
    /// tape records.
    /// </summary>
    internal enum CbmFileType
    {
        Deleted = 0,
        Sequential = 1,
        Program = 2,
        User = 3,
        Relative = 4
    }
}
