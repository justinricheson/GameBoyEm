using GameBoyEm.Interfaces;

namespace GameBoyEm.Cartridge
{
    public abstract class Cartridge : ICartridge
    {
        public abstract byte Read(ushort address);
        public abstract void Write(ushort address, byte value);
    }
}
