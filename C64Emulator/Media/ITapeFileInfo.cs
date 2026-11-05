namespace C64Emulator.Media;

/// <summary>
/// Metadata common to files decoded from a T64 or TAP tape image.
/// The payload itself stays with the image that owns the file.
/// </summary>
internal interface ITapeFileInfo
{
    string Name { get; }

    CbmFileType Type { get; }

    bool IsProgram { get; }
}
