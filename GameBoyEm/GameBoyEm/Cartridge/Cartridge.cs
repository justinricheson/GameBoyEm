using GameBoyEm.Api;
using System.Linq;

namespace GameBoyEm.Cartridge
{
    public abstract class Cartridge : ICartridge
    {
        protected const ushort RomBankSize = 16384;
        protected const ushort TypeAddress = 0x0147;
        protected const byte RomOnlyType = 0x00;
        protected byte[] _rom;

        public Cartridge(byte[] rom)
        {
            _rom = rom;
        }

        public static CartridgeType GetType(byte[] rom)
        {
            var type = rom[TypeAddress];
            return type == RomOnlyType
                ? CartridgeType.RomOnly
                : CartridgeType.Unknown;
        }
        public abstract byte Read(ushort address);
        public abstract void Write(ushort address, byte value);
    }

    public enum CartridgeType
    {
        Unknown,
        RomOnly
    }
}
