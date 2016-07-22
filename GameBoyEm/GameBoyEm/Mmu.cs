using System;
using GameBoyEm.Api;

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

        // Interrupt Notes:
        // FFFF - controls whether cpu should respond to a triggered interrupt
        // Bit Map
        // 0 Vblank
        // 1 LCD Stat
        // 2 Timer
        // 3 Serial
        // 4 Joypad
        // FF0F - controls whether an interrupt has been triggered, same bit order as FFFF

        // Note: On the real system, cartridges could contain a rombank chip that swapped rom
        // banks in as needed. Only one bank is addressable at a time via this address range

        // Note: Due to how the gameboy hardware is constructed, a shadow of the working ram
        // is available in the addresses directly above the working ram (except the last 512 bytes).
        // This is replicated so games that took advantage of this quirk are still compatible

        // Don't see much benefit in breaking up the memory which will require offsets to read and write.
        // Will prob regret that decision as soon as it's too late to change
        // Note: 0000 - 7FFF is stored in the cartridge, but we keep an empty block at the beginning
        // of _memory to make it easier to address higher memory
        private byte[] _memory;
        private ICartridge _cartridge;

        public byte Interrupts
        {
            get
            {
                var interruptFlags = ReadByte(0xFF0F);
                var interruptMasks = ReadByte(0xFFFF);
                return (byte)(interruptFlags & interruptMasks & 0x1F);
            }
        }
        public bool InterruptsExist { get { return Interrupts > 1; } }
        public bool Vblank
        {
            get
            {
                var flag = (Interrupts & 0x01) == 0x01;
                WriteByte(0xFF0F, (byte)(ReadByte(0xFF0F) & 0xFE)); // Disable Flag
                return flag;
            }
        }
        public bool LcdStat
        {
            get
            {
                var flag = (Interrupts & 0x02) == 0x02;
                WriteByte(0xFF0F, (byte)(ReadByte(0xFF0F) & 0xFD)); // Disable Flag
                return flag;
            }
        }
        public bool Timer
        {
            get
            {
                var flag = (Interrupts & 0x04) == 0x04;
                WriteByte(0xFF0F, (byte)(ReadByte(0xFF0F) & 0xFB)); // Disable Flag
                return flag;
            }
        }
        public bool Serial
        {
            get
            {
                var flag = (Interrupts & 0x08) == 0x08;
                WriteByte(0xFF0F, (byte)(ReadByte(0xFF0F) & 0xF7)); // Disable Flag
                return flag;
            }
        }
        public bool JoyPad
        {
            get
            {
                var flag = (Interrupts & 0x10) == 0x10;
                WriteByte(0xFF0F, (byte)(ReadByte(0xFF0F) & 0xEF)); // Disable Flag
                return flag;
            }
        }

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
        public void LoadCartridge(ICartridge cartridge) => _cartridge = cartridge;

        public byte ReadByte(ushort address)
        {
            if (address <= 0x7FFF)
            {
                return _cartridge.Read(address);
            }
            return _memory[address];
        }
        public ushort ReadWord(ushort address)
        {
            return ReadByte(address).ToShort()
               .OR(ReadByte(++address).ToShort().LS(8));
        }
        public void WriteByte(ushort address, byte value)
        {
            if (address <= 0x7FFF)
            {
                _cartridge.Write(address, value);
                return;
            }

            _memory[address] = value;
        }
        public void WriteWord(ushort address, ushort value)
        {
            WriteByte(address, value.AND(0x00FF).ToByte());
            WriteByte(++address, value.AND(0xFF00).RS(8).ToByte());
        }
    }
}
