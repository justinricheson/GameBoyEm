using System;
using GameBoyEm.Interfaces;

namespace GameBoyEm
{
    public class Mmu : IMmu
    {
        // Memory Map
        // 0000 - 3FFF (cartridge rom)
        // 4000 - 7FFF (swappable rom banks)
        // 8000 - 9FFF (video ram)
        // A000 - BFFF (external ram)
        // C000 - DFFF (working ram)
        // E000 - FDFF (shadow ram)
        // FE00 - FE9F (sprite info, positions, etc)
        // FEA0 - FEFF (wat? what's this used for??)
        // FF00 - FF7F (memory mapped IO)
        // FF80 - FFFE (high-speed zero-page)
        // FFFF (interrupt enable register)

        // Note: On the real system, cartridges could contain a rombank chip that swapped rom
        // banks in as needed. Only one bank is addressable at a time via this address range

        // Note: Due to how the gameboy hardware is constructed, a shadow of the working ram
        // is available in the addresses directly above the working ram (except the last 512 bytes).
        // This is replicated so games that took advantage of this quirk are still compatible

        // Don't see much benefit to breaking up the memory by
        // section which will require offsets to read and write
        // I'll prob regret this decision once it's too late to change
        private byte[] _memory;

        public Mmu()
        {
            Reset();
        }

        public void Reset()
        {
            _memory = new byte[65536];

            // Magic init code obtained from https://github.com/boeker/gameboy/blob/master/src/gameboy/memory.cpp
            // Sets up io memory without having to execute the proprietary gameboy bios rom
            _memory[0xFF05] = 0x00;
            _memory[0xFF06] = 0x00;
            _memory[0xFF07] = 0x00;
            _memory[0xFF10] = 0x80;
            _memory[0xFF11] = 0xBF;
            _memory[0xFF12] = 0xF3;
            _memory[0xFF14] = 0xBF;
            _memory[0xFF16] = 0x3F;
            _memory[0xFF17] = 0x00;
            _memory[0xFF19] = 0xBF;
            _memory[0xFF1A] = 0x7F;
            _memory[0xFF1B] = 0xFF;
            _memory[0xFF1C] = 0x9F;
            _memory[0xFF1E] = 0xBF;
            _memory[0xFF20] = 0xFF;
            _memory[0xFF21] = 0x00;
            _memory[0xFF22] = 0x00;
            _memory[0xFF23] = 0xBF;
            _memory[0xFF24] = 0x77;
            _memory[0xFF25] = 0xF3;
            _memory[0xFF26] = 0xF1;
            _memory[0xFF40] = 0x91;
            _memory[0xFF42] = 0x00;
            _memory[0xFF43] = 0x00;
            _memory[0xFF45] = 0x00;
            _memory[0xFF47] = 0xFC;
            _memory[0xFF48] = 0xFF;
            _memory[0xFF49] = 0xFF;
            _memory[0xFF4A] = 0x00;
            _memory[0xFF4B] = 0x00;
            _memory[0xFFFF] = 0x00;
        }

        public void LoadCartridge(ICartridge cartridge)
        {
            throw new NotImplementedException();
        }

        public byte ReadByte(ushort address)
        {
            throw new NotImplementedException();
        }

        public ushort ReadWord(ushort address)
        {
            throw new NotImplementedException();
        }

        public void WriteByte(ushort address, byte value)
        {
            throw new NotImplementedException();
        }

        public void WriteWord(ushort address, ushort value)
        {
            throw new NotImplementedException();
        }
    }
}
