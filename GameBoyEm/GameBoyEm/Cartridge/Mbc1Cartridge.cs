using System;
using System.Runtime.Serialization;

namespace GameBoyEm.Cartridge
{
    [Serializable]
    public class Mbc1Cartridge : Cartridge
    {
        private byte[][] _romBanks;
        private ushort _romBankIndex = 1;

        public Mbc1Cartridge(byte[] rom, byte[][] romBanks)
            : base(rom)
        {
            _romBanks = romBanks;
        }

        protected Mbc1Cartridge(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx)
        {
            _romBankIndex = info.GetUInt16("RomBankIndex");
            _romBanks = (byte[][])info.GetValue("RomBanks", typeof(byte[][]));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("RomBankIndex", _romBankIndex);
            info.AddValue("RomBanks", _romBanks);
        }

        public override byte Read(ushort address)
        {
            if (address <= 0x3FFF)
            {
                return _rom[address];
            }
            var offsetAddress = address - BankSize; // Skip rom
            return _romBanks[_romBankIndex][offsetAddress];
        }
        
        public override void Write(ushort address, byte value)
        {
            if (address >= 0x2000 && address <= 0x3FFF)
            {
                byte lowerFiveBits = value.AND(0x1F);
                _romBankIndex = _romBankIndex.AND(0xE0).OR(lowerFiveBits);
            }
            else if (address >= 0x4000 && address <= 0x5FFF)
            {
                byte upperTwoBits = value.AND(0x03);
                _romBankIndex = _romBankIndex.AND(0x1F).OR(upperTwoBits.LS(5));
            }

            // Invalid bank indexes jump to the next index
            if (_romBankIndex == 0x00
             || _romBankIndex == 0x20
             || _romBankIndex == 0x40
             || _romBankIndex == 0x60)
            {
                _romBankIndex++;
            }
        }
    }
}
