using GameBoyEm.Interfaces;
using System;
using System.Collections.Generic;

namespace GameBoyEm
{
    /// <summary>
    /// Emulates a Sharp LR35902 (modified Zilog Z80),
    /// 4.194304MHz, 8-bit processor used in the original GameBoy.
    /// Notes: IO is memory mapped. Memory is byte addressable with
    /// 16-bit addresses (64KB total memory).
    /// </summary>
    public class Cpu : ICpu
    {
        // Registers
        public byte A { get; private set; } // Accumulator
        public byte B { get; private set; } // General Purpose
        public byte C { get; private set; } // ..
        public byte D { get; private set; }
        public byte E { get; private set; }
        public byte H { get; private set; }
        public byte L { get; private set; }
        public byte F { get; private set; } // Flags (bits 0-3: unused, 4: carry, 5: half-carry, 6: subtract, 7: zero)
        public byte SP { get; private set; } // Stack pointer
        public ushort PC { get; private set; } // Program counter
        private bool IME { get; set; } // Interrupt master enable

        // Pseudo Registers
        public ushort BC { get { return (ushort)((B << 8) + C); } private set { B = (byte)(value >> 8); C = (byte)(value & 255); } }
        public ushort DE { get { return (ushort)((D << 8) + E); } private set { D = (byte)(value >> 8); E = (byte)(value & 255); } }
        public ushort HL { get { return (ushort)((H << 8) + L); } private set { H = (byte)(value >> 8); L = (byte)(value & 255); } }

        // Flag Accessors
        public bool FC { get { return (F & 16) != 0; } private set { if (value) F |= 16; else F &= 239; } }
        public bool FH { get { return (F & 32) != 0; } private set { if (value) F |= 32; else F &= 223; } }
        public bool FS { get { return (F & 64) != 0; } private set { if (value) F |= 64; else F &= 191; } }
        public bool FZ { get { return (F & 128) != 0; } private set { if (value) F |= 128; else F &= 127; } }

        // Timers (last instruction)
        private ulong M; // 1/4 Instruction time
        private ulong T { get { return M << 2; } } // Instruction time

        // Cumulative instruction timers
        private ulong _totalM;
        private ulong _totalT { get { return _totalM << 2; } }

        private IMmu _mmu;
        private List<Action> _ops;

        public Cpu(IMmu mmu, byte a = 0, byte b = 0, byte c = 0, byte d = 0, byte e = 0, byte h = 0, byte l = 0, byte f = 0, byte sp = 0, byte pc = 0)
        {
            A = a;
            B = b;
            C = c;
            D = d;
            E = e;
            H = h;
            L = l;
            F = f;
            SP = sp;
            PC = pc;

            _mmu = mmu;
            _ops = new List<Action>
            {
                /* 00 */ NOP, LDBCN, LDBCA, INCBC, INCB, DECB, LDNB, RLCA, LDNSP, ADDHLBC
                /* 10 */ // TODO
            };
        }

        public void Step()
        {
            var op = _mmu.ReadByte(PC++); // Fetch
            _ops[op](); // Decode, Execute

            _totalM += M;
        }

        #region Operations
        private void NOP() { M = 1; }

        // 8-bit Loads
        private void LDBCA() { WB(BC, A); M = 2; }
        private void LDNB() { B = RB(PC++); M = 2; }

        // 8-bit Arithmetic
        private void INCB() { B++; F = 0; SetZ(B); M = 1; }
        private void DECB() { B--; F = 0; SetZ(B); M = 1; }

        // 16-bit Loads
        private void LDBCN() { C = RB(PC++); B = RB(PC++); M = 3; }
        private void LDNSP() { WB(RW(PC), SP); PC += 2; M = 5; }

        // 16-bit Arithmetic
        private void INCBC() { C++; if (C == 0) B++; M = 1; }
        private void ADDHLBC() { int hl = HL + BC; HL = (ushort)hl; FC = hl > 65535; M = 3; } // TODO Implement half carry flag

        // Rotates and Shifts
        private void RLCA() { var hi = A.RS(7); A = hi.OR(A.LS(1)); F = 0; SetZ(A); SetC(hi); M = 1; }
        #endregion

        #region Helpers
        // MMU Helpers
        private byte RB(ushort address)
        {
            return _mmu.ReadByte(address);
        }
        private void WB(ushort address, byte value)
        {
            _mmu.WriteByte(address, value);
        }
        private ushort RW(ushort address)
        {
            return _mmu.ReadWord(address);
        }
        private void WW(ushort address, ushort value)
        {
            _mmu.WriteWord(address, value);
        }

        // Flag Helpers
        private void SetC(byte value)
        {
            if (value == 1)
            {
                F |= 16;
            }
        }
        private void SetZ(byte value)
        {
            if (value == 0)
            {
                F |= 128;
            }
        }
        #endregion
    }
}
