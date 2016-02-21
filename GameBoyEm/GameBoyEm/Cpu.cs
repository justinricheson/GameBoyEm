using System;
using System.Collections.Generic;

namespace GameBoyEm
{
    /// <summary>
    /// Emulates the modified Zilog Z80 (aka Sharp LR35902),
    /// 4.194304MHz, 8-bit processor used in the original GameBoy.
    /// Notes: IO is memory mapped. Memory is byte addressable with
    /// 16 bit addresses (64KB total memory).
    /// </summary>
    public class Cpu
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
        public bool IME { get; private set; } // Interrupt master enable

        // Timers (last instruction)
        public ulong M { get; private set; }
        public ulong T { get; private set; }

        // Cumulative instruction timers
        public ulong TotalM { get; private set; }
        public ulong TotalT { get; private set; }

        private IMmu _mmu;
        private List<Action> _ops;

        public Cpu(IMmu mmu)
        {
            _mmu = mmu;
            _ops = new List<Action>
            {
                NOP,
                LDBCNN,
                LDBCA
            };
        }

        public void Run()
        {
            Init();
            while (true)
            {
                var op = _mmu.ReadByte(PC++); // Fetch
                _ops[op](); // Decode, Execute

                TotalM += M;
                TotalT += T;
            }
        }

        private void Init()
        {
            A = B = C = D = E = H = L = F = SP = 0;
            M = T = TotalM = TotalT = 0;
            PC = 0;
            IME = false;
        }

        private void NOP() { M = 1; T = 4; }

        // 8-bit Loads
        private void LDBCA() { _mmu.WriteByte((ushort)((B << 8) + C), A); }

        // 16-bit Loads
        private void LDBCNN() { }
    }
}
