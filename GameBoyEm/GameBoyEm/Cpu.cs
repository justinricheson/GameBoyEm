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
                A = a, B = b, C = c, D = d,
                E = e, H = h, L = l, F = f,
                SP = sp, PC = pc
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
            public ushort BC => (ushort)((B << 8) + C);
            public ushort DE => (ushort)((D << 8) + E);
            public ushort HL => (ushort)((H << 8) + L);

            // Timers (last instruction)
            private ulong M;
            private ulong T;

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
                    /* 00 */ NOP, LDBCNN, LDBCA, INCBC, INCB, DECB, LDNB
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
            private void NOP() { M = 1; T = 4; }

            // 8-bit Loads
            private void LDBCA() { WB(BC, A); M = 2; T = 8; }
            private void LDNB() { B = RB(PC++); M = 2; T = 8; }

            // 8-bit Arithmetic
            private void INCB() { B++; TrySetZ(B); M = 1; T = 4; }
            private void DECB() { B--; TrySetZ(B); M = 1; T = 4; }

            // 16-bit Loads
            private void LDBCNN() { C = RB(PC++); B = RB(PC++); M = 3; T = 12; }

            // 16-bit Arithmetic
            private void INCBC() { Inc16(ref B, ref C); M = 1; T = 4; }
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

            // 16-bit Helpers
            private void Inc16(ref byte high, ref byte low)
            {
                low++;
                if (low == 0)
                {
                    high++;
                }
            }

            // Flag Helpers
            private void TrySetZ(byte value, bool clearFlags = true)
            {
                TryClearFlags(clearFlags);
                if (value == 0)
                {
                    F |= 128;
                }
            }
            private void TryClearFlags(bool clearFlags)
            {
                if (clearFlags)
                {
                    F = 0;
                }
            }
            #endregion
        }
    }
}
