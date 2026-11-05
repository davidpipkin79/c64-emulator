using System;
using System.IO;

namespace C64Emulator.Core;

internal static class RomImage
{
    public static byte[] Load(string path, int expectedSize, string name)
    {
        try
        {
            var data = File.ReadAllBytes(path);

            if (data.Length != expectedSize)
            {
                throw new InvalidDataException(
                    $"{name} must be exactly {expectedSize} bytes; found {data.Length} at {path}."
                );
            }

            return data;
        }
        catch (IOException ex)
        {
            throw new InvalidDataException($"{name} could not be read from {path}.", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new InvalidDataException($"{name} is not readable at {path}.", ex);
        }
    }
}
