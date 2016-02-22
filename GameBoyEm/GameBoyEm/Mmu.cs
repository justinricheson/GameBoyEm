using System;
using GameBoyEm.Interfaces;

namespace GameBoyEm
{
    public class Mmu : IMmu
    {
        private byte[] _memory;

        public Mmu(params byte[] load)
        {
            _memory = new byte[65535];

            int length = load == null ? 0
                : Math.Min(load.Length, _memory.Length);
            for (int i = 0; i < length; i++)
            {
                _memory[i] = load[i];
            }
        }

        public byte ReadByte(ushort address)
        {
            return _memory[address];
        }

        public ushort ReadWord(ushort address)
        {
            // TODO not sure if this is the right order
            byte high = _memory[address];
            byte low = _memory[address + 1];

            return (ushort)((high << 8) + low);
        }

        public void WriteByte(ushort address, byte value)
        {
            _memory[address] = value;
        }

        public void WriteWord(ushort address, ushort value)
        {
            byte high = (byte)(value >> 8);
            byte low = (byte)(value & 255);

            // TODO not sure if this is the right order
            _memory[address] = high;
            _memory[address + 1] = low;
        }
    }
}
