using System;
using System.IO;

namespace C64Emulator
{
    /// <summary>
    /// The three ROM images required by a stock C64. Loading is kept separate
    /// from chip emulation so a machine can be assembled with test images.
    /// </summary>
    internal sealed class RomSet
    {
        private RomSet(byte[] kernal, byte[] basic, byte[] character)
        {
            Kernal = kernal;
            Basic = basic;
            Character = character;
        }

        public byte[] Kernal { get; }

        public byte[] Basic { get; }

        public byte[] Character { get; }

        public static RomSet Load(string directory)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(directory);

            return new RomSet(
                RomImage.Load(
                    Path.Combine(directory, "kernal.rom"),
                    AddressMap.KernalRomSize,
                    "KERNAL.ROM"),
                RomImage.Load(
                    Path.Combine(directory, "basic.rom"),
                    AddressMap.BasicRomSize,
                    "BASIC.ROM"),
                RomImage.Load(
                    Path.Combine(directory, "char.rom"),
                    AddressMap.CharacterRomSize,
                    "CHAR.ROM"));
        }

        /// <summary>
        /// Creates a ROM set from in-memory images, primarily for tests.
        /// </summary>
        public static RomSet Create(byte[] kernal, byte[] basic, byte[] character)
        {
            ArgumentNullException.ThrowIfNull(kernal);
            ArgumentNullException.ThrowIfNull(basic);
            ArgumentNullException.ThrowIfNull(character);

            if (kernal.Length != AddressMap.KernalRomSize)
                throw new ArgumentException("KERNAL image has the wrong size.", nameof(kernal));

            if (basic.Length != AddressMap.BasicRomSize)
                throw new ArgumentException("BASIC image has the wrong size.", nameof(basic));

            if (character.Length != AddressMap.CharacterRomSize)
                throw new ArgumentException("Character image has the wrong size.", nameof(character));

            return new RomSet(kernal, basic, character);
        }
    }
}
