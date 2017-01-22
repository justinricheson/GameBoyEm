using System;
using System.Runtime.Serialization;

namespace GameBoyEm.Cartridge
{
    [Serializable]
    public class Mbc1Cartridge : Cartridge
    {
        private byte[][] _banks;
        private ushort _bankIndex = 1;
        private bool _ramEnabled;

        public Mbc1Cartridge(byte[] rom, byte[][] banks) : base(rom)
        {
            _banks = banks;
        }

        protected Mbc1Cartridge(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx)
        {
            _bankIndex = (ushort)info.GetValue("BankIndex", typeof(ushort));
            _banks = (byte[][])info.GetValue("Banks", typeof(byte[][]));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("BankIndex", _bankIndex);
            info.AddValue("Banks", _banks);
        }

        public override byte Read(ushort address)
        {
            if (address <= 0x3FFF)
            {
                return _rom[address];
            }
            var offsetAddress = address - BankSize; // Skip rom
            return _banks[_bankIndex][offsetAddress];
        }
        
        public override void Write(ushort address, byte value)
        {
            if (address < 0x2000)
            {
                var lowerFourBits = value.AND(0x0F);
                _ramEnabled = lowerFourBits == 0x0A;
            }
            else if (address >= 0x2000 && address <= 0x3FFF)
            {
                byte lowerFiveBits = value.AND(0x1F);
                _bankIndex = _bankIndex.AND(0xE0).OR(lowerFiveBits);
            }
            else if (address >= 0x4000 && address <= 0x5FFF)
            {
                byte upperTwoBits = value.AND(0x03);
                _bankIndex = _bankIndex.AND(0x1F).OR(upperTwoBits.LS(5));
            }

            if (_bankIndex == 0x00
             || _bankIndex == 0x20
             || _bankIndex == 0x40
             || _bankIndex == 0x60)
            {
                _bankIndex++;
            }
        }
    }
}
