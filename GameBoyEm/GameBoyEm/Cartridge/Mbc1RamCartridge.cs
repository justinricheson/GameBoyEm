using System;
using System.Runtime.Serialization;

namespace GameBoyEm.Cartridge
{
    [Serializable]
    public class Mbc1RamCartridge : Mbc1Cartridge
    {
        private bool _enabled;
        private bool _ramEnabled;
        private ushort _ramBankIndex;
        private byte[][] _ramBanks;

        public Mbc1RamCartridge(byte[] rom, byte[][] romBanks, byte[][] ramBanks)
            : base(rom, romBanks)
        {
            _ramBanks = ramBanks;
        }

        protected Mbc1RamCartridge(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx)
        {
            _enabled = info.GetBoolean("Enabled");
            _ramEnabled = info.GetBoolean("RamEnabled");
            _ramBankIndex = info.GetUInt16("RamBankIndex");
            _ramBanks = (byte[][])info.GetValue("RamBanks", typeof(byte[][]));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Enabled", _enabled);
            info.AddValue("RamEnabled", _ramEnabled);
            info.AddValue("RamBankIndex", _ramBankIndex);
            info.AddValue("RamBanks", _ramBanks);
        }

        public override void Write(ushort address, byte value)
        {
            if (address <= 0x1FFF)
            {
                _enabled = value.AND(0x0A) == 0x0A;
            }
            else if (address >= 0x6000 && address <= 0x7FFF)
            {
                _ramEnabled = value == 0;
            }
            else if (_ramEnabled && address >= 0x4000 && address <= 0x5FFF)
            {
                _ramBankIndex = value.AND(0x03);
            }
            else
            {
                base.Read(address);
            }
        }

        public override byte ReadRam(ushort address)
        {
            var offsetAddress = address - 0xA000;
            return _enabled ? _ramBanks[_ramBankIndex][offsetAddress] : (byte)0x00;
        }

        public override void WriteRam(ushort address, byte value)
        {
            if (_enabled)
            {
                var offsetAddress = address - 0xA000;
                _ramBanks[_ramBankIndex][offsetAddress] = value;
            }
        }
    }
}
