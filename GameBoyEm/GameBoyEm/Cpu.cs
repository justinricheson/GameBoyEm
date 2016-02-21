using System;
using System.Collections.Generic;

namespace GameBoyEm
{
    /// <summary>
    /// Emulates a modified Zilog Z80 (aka Sharp LR35902),
    /// 4.194304MHz, 8-bit processor used in the original GameBoy.
    /// Notes: IO is memory mapped. Memory is byte addressable with
    /// 16-bit addresses (64KB total memory).
    /// </summary>
    public partial class Cpu
    {
        // Registers
        private byte _a; public byte A => _a; // Accumulator
        private byte _b; public byte B => _b; // General Purpose
        private byte _c; public byte C => _c; // ..
        private byte _d; public byte D => _d;
        private byte _e; public byte E => _e;
        private byte _h; public byte H => _h;
        private byte _l; public byte L => _l;
        private byte _f; public byte F => _f; // Flags (bits 0-3: unused, 4: carry, 5: half-carry, 6: subtract, 7: zero)
        private byte _sp; public byte SP => _sp; // Stack pointer
        private ushort _pc; public ushort PC => _pc; // Program counter
        private bool _ime; public bool IME => _ime; // Interrupt master enable

        // Pseudo Registers
        public ushort BC => (ushort)((_b << 8) + _c);
        public ushort DE => (ushort)((_d << 8) + _e);
        public ushort HL => (ushort)((_h << 8) + _l);

        // Timers (last instruction)
        private ulong M;
        private ulong T;

        // Cumulative instruction timers
        private ulong _totalM;
        private ulong _totalT;

        private IMmu _mmu;
        private List<Action> _ops;

        public Cpu(IMmu mmu)
        {
            _mmu = mmu;
            _ops = new List<Action>
            {
                /* 00 */ NOP, LDBCNN, LDBCA, INCBC, INCB, DECB, LDNB
                /* 10 */ // TODO
            };
        }

        public void Run()
        {
            Init();
            while (true)
            {
                var op = _mmu.ReadByte(_pc++); // Fetch
                _ops[op](); // Decode, Execute

                _totalM += M;
                _totalT += T;
            }
        }

        private void Init()
        {
            _a = _b = _c = _d = _e = _h = _l = _f = _sp = 0;
            M = T = _totalM = _totalT = 0;
            _pc = 0;
            _ime = false;
        }
    }
}
