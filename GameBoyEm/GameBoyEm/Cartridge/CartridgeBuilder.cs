using GameBoyEm.Api;
using System;
using System.Linq;

namespace GameBoyEm.Cartridge
{
    public static class CartridgeBuilder
    {
        private const ushort _romSizeAddress = 0x0148;
        private const ushort _typeAddress = 0x0147;
        private const byte _romOnlyType = 0x00;
        private const byte _mbc1Type = 0x01;

        public static ICartridge Build(byte[] rom)
        {
            var cartridgeType = rom[_typeAddress];
            switch (cartridgeType)
            {
                case _romOnlyType:
                    return new RomOnlyCartridge(rom);
                case _mbc1Type:
                    return new Mbc1Cartridge(rom, GetBanks(rom));
                // TODO
                default:
                    throw new NotSupportedException("Unsupported cartridge type");
            }
        }

        private static byte[][] GetBanks(byte[] rom)
        {
            int numBanks;
            var romSize = rom[_romSizeAddress];
            switch (romSize)
            {
                case 0x52:
                    numBanks = 72;
                    break;
                case 0x53:
                    numBanks = 80;
                    break;
                case 0x54:
                    numBanks = 96;
                    break;
                default:
                    numBanks = (int)Math.Pow(2, romSize + 1);
                    break;
            }

            var banks = new byte[numBanks][];
            banks[0] = rom.Take(Cartridge.BankSize).ToArray();
            for (int i = 1; i < numBanks; i++)
            {
                banks[i] = rom
                    .Skip(Cartridge.BankSize * i)
                    .Take(Cartridge.BankSize)
                    .ToArray();
            }

            return banks;
        }
    }
}
