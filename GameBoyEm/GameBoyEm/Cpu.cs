﻿using GameBoyEm.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameBoyEm.CpuCycles;

namespace GameBoyEm
{
    /// <summary>
    /// Emulates a Sharp LR35902 (modified Zilog Z80),
    /// 4.194304MHz, 8-bit processor used in the original GameBoy.
    /// Notes: IO is memory mapped. Memory is byte addressable with
    /// 16-bit addresses (64KB total memory).
    /// </summary>
    public partial class Cpu : ICpu
    {
        private IMmu _mmu;
        private List<Action> _ops;
        private List<Action> _cbOps;

        // Registers
        public byte A { get; private set; } // Accumulator
        public byte B { get; private set; } // General Purpose
        public byte C { get; private set; } // ..
        public byte D { get; private set; }
        public byte E { get; private set; }
        public byte H { get; private set; }
        public byte L { get; private set; }
        public byte F { get; private set; } // Flags (bits 0-3: unused, 4: carry, 5: half-carry, 6: subtract, 7: zero)
        public ushort SP { get; private set; } // Stack pointer
        public ushort PC { get; private set; } // Program counter
        private bool IME { get; set; } // Interrupt master enable

        // Pseudo Registers
        public ushort BC { get { return (ushort)((B << 8) + C); } private set { B = (byte)(value >> 8); C = (byte)(value & 255); } }
        public ushort DE { get { return (ushort)((D << 8) + E); } private set { D = (byte)(value >> 8); E = (byte)(value & 255); } }
        public ushort HL { get { return (ushort)((H << 8) + L); } private set { H = (byte)(value >> 8); L = (byte)(value & 255); } }

        // Flag Accessors
        public bool FC { get { return (F & 16) != 0; } private set { if (value) F |= 16; else F &= 239; } }
        public bool FH { get { return (F & 32) != 0; } private set { if (value) F |= 32; else F &= 223; } }
        public bool FN { get { return (F & 64) != 0; } private set { if (value) F |= 64; else F &= 191; } }
        public bool FZ { get { return (F & 128) != 0; } private set { if (value) F |= 128; else F &= 127; } }

        // Cumulative instruction timers
        private ulong _totalM;
        private ulong _totalT { get { return _totalM << 2; } }

        // Constants
        private static readonly byte _u4 = 15;
        private static readonly byte _u8 = byte.MaxValue;
        private static readonly ushort _u12 = 4095;
        private static readonly ushort _u16 = ushort.MaxValue;

        public void Step()
        {
            Step(_ops, Cycles);
        }

        private void Step(List<Action> ops, IReadOnlyCollection<byte> cycleTimes)
        {
            var op = _mmu.ReadByte(PC++); // Fetch
            ops[op](); // Decode, Execute
            _totalM += cycleTimes.ElementAt(op);
        }

        public Cpu(IMmu mmu,
            byte a = 0, byte b = 0, byte c = 0, byte d = 0,
            byte e = 0, byte h = 0, byte l = 0, byte f = 0,
            byte sp = 0, byte pc = 0)
        {
            A = a; B = b; C = c; D = d;
            E = e; H = h; L = l; F = f;
            SP = sp; PC = pc;

            _mmu = mmu;

            _ops = new List<Action>
            {
                /* 00 */ NOP,   LDBCN, LDBCA,  INCBC, INCB,   DECB,   LDBN,   RLCA, LDNSP, ADDHLBC, LDABC,  DECBC, INCC, DECC, LDCN, RRCA,
                /* 10 */ STOP,  LDDEN, LDDEA,  INCDE, INCD,   DECD,   LDDN,   RLA,  JRN,   ADDHLDE, LDADE,  DECDE, INCE, DECE, LDEN, RRA,
                /* 20 */ JRNZN, LDHLN, LDIHLA, INCHL, INCH,   DECH,   LDHN,   DAA,  JRZN,  ADDHLHL, LDIAHL, DECHL, INCL, DECL, LDLN, CPL,
                /* 30 */ JRNCN, LDSPN, LDDHLA, INCSP, INCHLM, DECHLM, LDHLMN, SCF,  SRLB,  ADDHLSP, LDDAHL, DECSP, INCA, DECA, LDAN, CCF
                /* 40 */ //LDBB, LDBC, LDBD, LDBE, LDBH, LDBL, LDBHL, LDBA, LDCB, LDCC, LDCD, LDCE, LDCH, LDCL, LDCHL, LDCA,
                /* 50 */ //LDDB, LDDC, LDDD, LDDE, LDDH, LDDL, LDDHL, LDDA, LDEB, LDEC, LDED, LDEE, LDEH, LDEL, LDEHL, LDEA,
                /* 60 */ //LDHB, LDHC, LDHD, LDHE, LDHH, LDHL, LDHHL, LDHA, LDLB, LDLC, LDLD, LDLE, LDLH, LDLL, LDLHL, LDLA,
                /* 70 */ //LDHLB, LDHLC, LDHLD, LDHLE, LDHLH, LDHLL, HALT, LDHLA, LDAB,  LDAC, LDAD, LDAE, LDAH, LDAL, LDHLA, LDAA,
                /* 80 */
                /* 90 */
                /* A0 */
                /* B0 */
                /* C0 */
                /* D0 */
                /* E0 */
                /* F0 */
            };

            _cbOps = new List<Action>
            {
                /* 00 */
                /* 10 */
                /* 20 */
                /* 30 */
                /* 40 */
                /* 50 */
                /* 60 */
                /* 70 */
                /* 80 */
                /* 90 */
                /* A0 */
                /* B0 */
                /* C0 */
                /* D0 */
                /* E0 */
                /* F0 */
            };
        }
    }
}
