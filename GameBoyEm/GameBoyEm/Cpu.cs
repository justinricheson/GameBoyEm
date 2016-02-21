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
        private byte A; // Accumulator
        private byte B; // General Purpose
        private byte C; // ..
        private byte D;
        private byte E;
        private byte H;
        private byte L;
        private byte F; // Flags (bits 0-3: unused, 4: carry, 5: half-carry, 6: subtract, 7: zero)
        private byte SP; // Stack pointer
        private ushort PC; // Program counter
        private bool IME; // Interrupt master enable

        // Pseudo Registers
        private ushort BC
        {
            get { return (ushort)((B << 8) + C); }
            set { B = (byte)(value >> 8); C = (byte)(value & 255); }
        }

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
                var op = _mmu.ReadByte(PC++); // Fetch
                _ops[op](); // Decode, Execute

                _totalM += M;
                _totalT += T;
            }
        }

        private void Init()
        {
            A = B = C = D = E = H = L = F = SP = 0;
            M = T = _totalM = _totalT = 0;
            PC = 0;
            IME = false;
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
