using System;

namespace GameBoyEm.Api
{
    public interface ICartridge
    {
        byte Read(ushort address);
        void Write(ushort address, byte value);
    }
}
