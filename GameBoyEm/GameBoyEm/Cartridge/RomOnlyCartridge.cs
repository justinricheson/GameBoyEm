using System;
using System.Runtime.Serialization;

namespace GameBoyEm.Cartridge
{
    [Serializable]
    public class RomOnlyCartridge : Cartridge
    {
        public RomOnlyCartridge(byte[] rom) : base(rom) { }
        protected RomOnlyCartridge(SerializationInfo info, StreamingContext ctx) : base(info, ctx) { }
        public override byte Read(ushort address) => _rom[address];
        public override void Write(ushort address, byte value) { } // Do nothing, rom writes are meant to swap the bank, but there is only one bank
    }
}
