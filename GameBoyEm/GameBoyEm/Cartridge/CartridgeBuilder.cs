using GameBoyEm.Api;
using System;
using System.Linq;

namespace GameBoyEm.Cartridge
{
    public static class CartridgeBuilder
    {
        private const ushort _romSizeAddress = 0x0148;
        private const ushort _ramSizeAddress = 0x0149;
        private const ushort _typeAddress = 0x0147;
        private const byte _romOnlyType = 0x00;
        private const byte _mbc1Type = 0x01;
        private const byte _mbc1RamType = 0x02;
        private const byte _mbc1RamBatteryType = 0x03;

        public static ICartridge Build(byte[] rom)
        {
            var cartridgeType = rom[_typeAddress];
            switch (cartridgeType)
            {
                case _romOnlyType:
                    return new RomOnlyCartridge(rom);
                case _mbc1Type:
                    return new Mbc1Cartridge(rom, GetRomBanks(rom));
                case _mbc1RamType:
                    return new Mbc1RamCartridge(rom, GetRomBanks(rom), GetRamBanks(rom));
                case _mbc1RamBatteryType:
                    return new Mbc1RamBatteryCartridge(rom, GetRomBanks(rom), GetRamBanks(rom));
                // TODO
                default:
                    throw new NotSupportedException("Unsupported cartridge type");
            }
        }

        private static byte[][] GetRomBanks(byte[] rom)
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

        private static byte[][] GetRamBanks(byte[] rom)
        {
            int numBanks = 0;
            int ramLength = 0;
            var ramSize = rom[_ramSizeAddress];
            switch (ramSize)
            {
                case 0x01:
                    numBanks = 1;
                    ramLength = 2048;
                    break;
                case 0x02:
                    numBanks = 1;
                    ramLength = 8192;
                    break;
                case 0x03:
                    numBanks = 4;
                    ramLength = 8192;
                    break;
            }

            var banks = new byte[numBanks][];
            for (int i = 0; i < numBanks; i++)
            {
                banks[i] = new byte[ramLength];
            }

            return banks;
        }
    }
}
