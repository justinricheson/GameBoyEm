using GameBoyEm.Api;
using System;
using System.Runtime.Serialization;

namespace GameBoyEm
{
    [Serializable]
    public class Mmu : IMmu, ISerializable
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
        // banks in as needed. Only one bank is addressable at a time via 4000 - 7FFF

        // Note: Due to how the gameboy hardware is constructed, a shadow of the working ram
        // is available in the addresses directly above the working ram (except the last 512 bytes).
        // This is replicated so games that took advantage of this quirk are still compatible

        // Don't see much benefit in breaking up the memory which will require offsets to read and write.
        // Will prob regret that decision as soon as it's too late to change
        // Note: 0000 - 7FFF is stored in the cartridge, but we keep an empty block at the beginning
        // of _memory to make it easier to address higher memory
        private byte[] _memory;
        private ICartridge _cartridge;

        // Interrupt Register
        public byte Interrupts
        {
            get
            {
                var interruptFlags = ReadByte(0xFF0F);
                var interruptMasks = ReadByte(0xFFFF);
                return (byte)(interruptFlags & interruptMasks & 0x1F);
            }
        }
        public bool InterruptsExist { get { return Interrupts > 0; } }
        public bool VblankInterrupt
        {
            get { return (Interrupts & 0x01) == 0x01; }
            set
            {
                if (value)
                {
                    WriteByte(0xFF0F, (byte)(ReadByte(0xFF0F) | 0x01));
                }
                else
                {
                    WriteByte(0xFF0F, (byte)(ReadByte(0xFF0F) & 0xFE));
                }
            }
        }
        public bool LcdStatInterrupt
        {
            get { return (Interrupts & 0x02) == 0x02; }
            set
            {
                if (value)
                {
                    WriteByte(0xFF0F, (byte)(ReadByte(0xFF0F) | 0x02));
                }
                else
                {
                    WriteByte(0xFF0F, (byte)(ReadByte(0xFF0F) & 0xFD));
                }
            }
        }
        public bool TimerInterrupt
        {
            get { return (Interrupts & 0x04) == 0x04; }
            set
            {
                if (value)
                {
                    WriteByte(0xFF0F, (byte)(ReadByte(0xFF0F) | 0x04));
                }
                else
                {
                    WriteByte(0xFF0F, (byte)(ReadByte(0xFF0F) & 0xFB));
                }
            }
        }
        public bool SerialInterrupt
        {
            get { return (Interrupts & 0x08) == 0x08; }
            set
            {
                if (value)
                {
                    WriteByte(0xFF0F, (byte)(ReadByte(0xFF0F) | 0x08));
                }
                else
                {
                    WriteByte(0xFF0F, (byte)(ReadByte(0xFF0F) & 0xF7));
                }
            }
        }
        public bool JoyPadInterrupt
        {
            get { return (Interrupts & 0x10) == 0x10; }
            set
            {
                if (value)
                {
                    WriteByte(0xFF0F, (byte)(ReadByte(0xFF0F) | 0x10));
                }
                else
                {
                    WriteByte(0xFF0F, (byte)(ReadByte(0xFF0F) & 0xEF));
                }
            }
        }

        // Controller Register
        public byte JoypadRegister { get { return ReadByte(0xFF00); } }
        public bool KeySelector { get { return (ReadByte(0xFF00) & 0x20) == 0; } }
        public bool DirSelector { get { return (ReadByte(0xFF00) & 0x10) == 0; } }
        public bool APressed
        {
            get { return !((JoypadRegister & 0x01) == 0x01); }
            set
            {
                if (value)
                {
                    WriteByte(0xFF00, ReadByte(0xFF00).AND(0xFE));
                }
                else
                {
                    WriteByte(0xFF00, ReadByte(0xFF00).OR(0x01));
                }
            }
        }
        public bool BPressed
        {
            get { return !((JoypadRegister & 0x02) == 0x02); }
            set
            {
                if (value)
                {
                    WriteByte(0xFF00, ReadByte(0xFF00).AND(0xFD));
                }
                else
                {
                    WriteByte(0xFF00, ReadByte(0xFF00).OR(0x02));
                }
            }
        }
        public bool SelectPressed
        {
            get { return !((JoypadRegister & 0x04) == 0x04); }
            set
            {
                if (value)
                {
                    WriteByte(0xFF00, ReadByte(0xFF00).AND(0xFB));
                }
                else
                {
                    WriteByte(0xFF00, ReadByte(0xFF00).OR(0x04));
                }
            }
        }
        public bool StartPressed
        {
            get { return !((JoypadRegister & 0x08) == 0x08); }
            set
            {
                if (value)
                {
                    WriteByte(0xFF00, ReadByte(0xFF00).AND(0xF7));
                }
                else
                {
                    WriteByte(0xFF00, ReadByte(0xFF00).OR(0x08));
                }
            }
        }
        public bool RightPressed
        {
            get { return !((JoypadRegister & 0x01) == 0x01); }
            set
            {
                if (value)
                {
                    WriteByte(0xFF00, ReadByte(0xFF00).AND(0xFE));
                }
                else
                {
                    WriteByte(0xFF00, ReadByte(0xFF00).OR(0x01));
                }
            }
        }
        public bool LeftPressed
        {
            get { return !((JoypadRegister & 0x02) == 0x02); }
            set
            {
                if (value)
                {
                    WriteByte(0xFF00, ReadByte(0xFF00).AND(0xFD));
                }
                else
                {
                    WriteByte(0xFF00, ReadByte(0xFF00).OR(0x02));
                }
            }
        }
        public bool UpPressed
        {
            get { return !((JoypadRegister & 0x04) == 0x04); }
            set
            {
                if (value)
                {
                    WriteByte(0xFF00, ReadByte(0xFF00).AND(0xFB));
                }
                else
                {
                    WriteByte(0xFF00, ReadByte(0xFF00).OR(0x04));
                }
            }
        }
        public bool DownPressed
        {
            get { return !((JoypadRegister & 0x08) == 0x08); }
            set
            {
                if (value)
                {
                    WriteByte(0xFF00, ReadByte(0xFF00).AND(0xF7));
                }
                else
                {
                    WriteByte(0xFF00, ReadByte(0xFF00).OR(0x08));
                }
            }
        }

        // LCD Control Register
        public byte LcdcRegister { get { return ReadByte(0xFF40); } }
        public bool LcdEnabled { get { return LcdcRegister.AND(0x80) != 0; } }
        public bool DisplayBackground { get { return LcdcRegister.AND(0x01) != 0; } }
        public bool DisplaySprites { get { return LcdcRegister.AND(0x02) != 0; } }
        public bool DisplayWindow { get { return LcdcRegister.AND(0x20) != 0; } }

        // LCD Status Register
        public byte LcdStatRegister
        {
            get { return ReadByte(0xFF41); }
            set { WriteByte(0xFF41, value); }
        }
        public Mode LcdMode
        {
            get { return (Mode)LcdStatRegister.AND(0x03); }
            set
            {
                var status = LcdStatRegister.AND(0xFC);
                LcdStatRegister = status.OR((byte)value);
            }
        }
        public bool CoincidenceFlag
        {
            get { return LcdStatRegister.AND(0x04) != 0; }
            set
            {
                if(value)
                {
                    WriteByte(0xFF41, LcdStatRegister.OR(0x04));
                }
                else
                {
                    WriteByte(0xFF41, LcdStatRegister.AND(0xFB));
                }
            }
        }
        public bool HBlankStatEnabled { get { return LcdStatRegister.AND(0x08) != 0; } }
        public bool VBlankStatEnabled { get { return LcdStatRegister.AND(0x10) != 0; } }
        public bool OamStatEnabled { get { return LcdStatRegister.AND(0x20) != 0; } }
        public bool CoincidenceStatEnabled { get { return LcdStatRegister.AND(0x40) != 0; } }

        // LCD Y-Coordinate Register
        public byte LcdCurrentLine
        {
            get { return ReadByte(0xFF44); }
            set { WriteByte(0xFF44, value); }
        }
        public byte LcdCurrentLineCompare { get { return ReadByte(0xFF45); } }

        // LCD Palette Register
        public byte LcdDefaultPalette { get { return ReadByte(0xFF47); } }

        // Timer Register
        public byte DividerRegister
        {
            get { return ReadByte(0xFF04); }
            set { _memory[0xFF04] = value; } // Don't use WriteByte so the value is not reset
        }
        public byte TimerCounter
        {
            get { return ReadByte(0xFF05); }
            set { WriteByte(0xFF05, value); }
        }
        public byte TimerModulo { get { return ReadByte(0xFF06); } }
        public byte TimerRegister { get { return ReadByte(0xFF07); } }
        public bool TimerEnabled { get { return TimerRegister.AND(0x04) != 0; } }
        public byte TimerSpeed { get { return TimerRegister.AND(0x03); } }

        public Mmu()
        {
            Reset();
        }

        protected Mmu(SerializationInfo info, StreamingContext ctx)
        {
            _memory = (byte[])info.GetValue("Memory", typeof(byte[]));
            _cartridge = (ICartridge)info.GetValue("Cartridge", typeof(ICartridge));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Memory", _memory);
            info.AddValue("Cartridge", _cartridge);
        }

        public void Reset()
        {
            _memory = new byte[65536];

            // Magic init code obtained from http://gbdev.gg8.se/wiki/articles/Pan_Docs
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
            else if (address == 0xFF04)
            {
                // Any write to divide register resets it
                value = 0;
            }
            else if (address == 0xFF46)
            {
                // DMA transfer, used to load OAM sprite information
                var start = ReadByte(0xFF46) * 0x100;
                for (int i = 0; i < 160; ++i)
                {
                    var source = (ushort)(start + i);
                    var target = (ushort)(0xFE00 + i);
                    WriteByte(target, ReadByte(source));
                }
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
