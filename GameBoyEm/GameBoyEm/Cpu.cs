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
        public byte A => _cpu.A;
        public byte B => _cpu.B;
        public byte C => _cpu.C;
        public byte D => _cpu.D;
        public byte E => _cpu.E;
        public byte H => _cpu.H;
        public byte L => _cpu.L;
        public byte F => _cpu.F;
        public byte SP => _cpu.SP;
        public ushort PC => _cpu.PC;
        public ushort BC => _cpu.BC;
        public ushort DE => _cpu.DE;
        public ushort HL => _cpu.HL;

        private InternalCpu _cpu;

        public Cpu(IMmu mmu, byte a = 0, byte b = 0, byte c = 0, byte d = 0, byte e = 0, byte h = 0, byte l = 0, byte f = 0, byte sp = 0, byte pc = 0)
        {
            _cpu = new InternalCpu(mmu)
            {
                A = a,
                B = b,
                C = c,
                D = d,
                E = e,
                H = h,
                L = l,
                F = f,
                SP = sp,
                PC = pc
            };
        }

        public void Step()
        {
            _cpu.Step();
        }

        /// <summary>
        /// Private class allows the register fields to be named the same as the wrapper properties.
        /// The operations are easier to read with standard register names, but still want to expose
        /// them as properties.
        /// </summary>
        private class InternalCpu
        {
            // Registers
            public byte A; // Accumulator
            public byte B; // General Purpose
            public byte C; // ..
            public byte D;
            public byte E;
            public byte H;
            public byte L;
            public byte F; // Flags (bits 0-3: unused, 4: carry, 5: half-carry, 6: subtract, 7: zero)
            public byte SP; // Stack pointer
            public ushort PC; // Program counter
            private bool IME; // Interrupt master enable

            // Pseudo Registers
            public ushort BC { get { return (ushort)((B << 8) + C); } set { B = (byte)(value >> 8); C = (byte)(value & 255); } }
            public ushort DE { get { return (ushort)((D << 8) + E); } set { D = (byte)(value >> 8); E = (byte)(value & 255); } }
            public ushort HL { get { return (ushort)((H << 8) + L); } set { H = (byte)(value >> 8); L = (byte)(value & 255); } }

            // Flag Accessors
            public bool FC { get { return (F & 16) != 0; } set { if (value) F |= 16; else F &= 239; } }
            public bool FH { get { return (F & 32) != 0; } set { if (value) F |= 32; else F &= 223; } }
            public bool FS { get { return (F & 64) != 0; } set { if (value) F |= 64; else F &= 191; } }
            public bool FZ { get { return (F & 128) != 0; } set { if (value) F |= 128; else F &= 127; } }

            // Timers (last instruction)
            private ulong M; // 1/4 Instruction time
            private ulong T; // Instruction time

            // Cumulative instruction timers
            private ulong _totalM;
            private ulong _totalT;

            private IMmu _mmu;
            private List<Action> _ops;

            public InternalCpu(IMmu mmu)
            {
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
                _totalT += T;
            }

            #region Operations
            private void NOP() { Tick1(); }

            // 8-bit Loads
            private void LDBCA() { WB(BC, A); Tick2(); }
            private void LDNB() { B = RB(PC++); Tick2(); }

            // 8-bit Arithmetic
            private void INCB() { B++; F = 0; SetZ(B); Tick1(); }
            private void DECB() { B--; F = 0; SetZ(B); Tick1(); }

            // 16-bit Loads
            private void LDBCN() { C = RB(PC++); B = RB(PC++); Tick3(); }
            private void LDNSP() { WB(RW(PC), SP); PC += 2; Tick5(); }

            // 16-bit Arithmetic
            private void INCBC() { C++; if (C == 0) B++; Tick1(); }
            private void ADDHLBC() { int hl = HL + BC; HL = (ushort)hl; FC = hl > 65535; Tick3(); } // TODO Implement half carry flag

            // Rotates and Shifts
            private void RLCA() { var hi = A.RS(7); A = hi.OR(A.LS(1)); F = 0; SetZ(A); SetC(hi); Tick1(); }
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

            // Clock Helpers (Note: hard coding the values avoids the division which
            // is important because these happen at the end of every instruction)
            private void Tick1()
            {
                T = 4;
                M = 1;
            }
            private void Tick2()
            {
                T = 8;
                M = 2;
            }
            private void Tick3()
            {
                T = 12;
                M = 3;
            }
            private void Tick4()
            {
                T = 16;
                M = 4;
            }
            private void Tick5()
            {
                T = 20;
                M = 5;
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
}
