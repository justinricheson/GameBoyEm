namespace GameBoyEm.Cartridge
{
    public class RomOnlyCartridge : Cartridge
    {
        public RomOnlyCartridge(byte[] rom) : base(rom) { }
        public override byte Read(ushort address) => _rom[address];

        // This is the easiest case, the bank never changes so we can write straight to the buffer
        public override void Write(ushort address, byte value) => _rom[address] = value;
    }
}
