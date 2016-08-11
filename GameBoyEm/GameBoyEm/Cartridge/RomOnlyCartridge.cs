namespace GameBoyEm.Cartridge
{
    public class RomOnlyCartridge : Cartridge
    {
        public RomOnlyCartridge(byte[] rom) : base(rom) { }
        public override byte Read(ushort address)// => _rom[address];
        {
            return _rom[address];
        }

        public override void Write(ushort address, byte value)
        {
            // Do nothing, rom writes are meant to swap the bank, but this only has one bank
            //_rom[address] = value;
        }
    }
}
