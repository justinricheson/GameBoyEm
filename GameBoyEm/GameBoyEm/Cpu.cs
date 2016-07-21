using GameBoyEm.Interfaces;
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
        public bool IME { get; private set; } // Interrupt master enable

        // Pseudo Registers
        public ushort AF { get { return (ushort)((A << 8) + F); } private set { A = (byte)(value >> 8); F = (byte)(value & 255); } }
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
        private static readonly ushort _io = 65280;

        public void Reset()
        {
            throw new NotImplementedException();
        }

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
            byte a = 0, byte b = 0, byte c = 0, byte d = 0, byte e = 0,
            byte h = 0, byte l = 0, ushort sp = 0, ushort pc = 0,
            bool fz = false, bool fn = false, bool fh = false, bool fc = false, bool ime = false)
        {
            A = a; B = b; C = c; D = d; E = e; H = h; L = l;
            FZ = fz; FN = fn; FH = fh; FC = fc; IME = ime;
            SP = sp; PC = pc;

            _mmu = mmu;

            _ops = new List<Action>
            {
                /* 00 */ NOP,     LDBCN,   LDBCA,   INCBC,   INCB,    DECB,    LDBN,     RLCA,    LDNSP,   ADDHLBC, LDABC,   DECBC,   INCC,    DECC,    LDCN,     RRCA,
                /* 10 */ STOP,    LDDEN,   LDDEA,   INCDE,   INCD,    DECD,    LDDN,     RLA,     JR,      ADDHLDE, LDADE,   DECDE,   INCE,    DECE,    LDEN,     RRA,
                /* 20 */ JRNZ,    LDHLN,   LDIHLA,  INCHL,   INCH,    DECH,    LDHN,     DAA,     JRZ,     ADDHLHL, LDIAHL,  DECHL,   INCL,    DECL,    LDLN,     CMPL,
                /* 30 */ JRNC,    LDSPN,   LDDHLA,  INCSP,   INCHLM,  DECHLM,  LDHLMN,   SCF,     JRC,     ADDHLSP, LDDAHL,  DECSP,   INCA,    DECA,    LDAN,     CCF,
                /* 40 */ LDBB,    LDBC,    LDBD,    LDBE,    LDBH,    LDBL,    LDBHL,    LDBA,    LDCB,    LDCC,    LDCD,    LDCE,    LDCH,    LDCL,    LDCHL,    LDCA,
                /* 50 */ LDDB,    LDDC,    LDDD,    LDDE,    LDDH,    LDDL,    LDDHL,    LDDA,    LDEB,    LDEC,    LDED,    LDEE,    LDEH,    LDEL,    LDEHL,    LDEA,
                /* 60 */ LDHB,    LDHC,    LDHD,    LDHE,    LDHH,    LDHL,    LDHHL,    LDHA,    LDLB,    LDLC,    LDLD,    LDLE,    LDLH,    LDLL,    LDLHL,    LDLA,
                /* 70 */ LDHLB,   LDHLC,   LDHLD,   LDHLE,   LDHLH,   LDHLL,   HALT,     LDHLA,   LDAB,    LDAC,    LDAD,    LDAE,    LDAH,    LDAL,    LDAHL,    LDAA,
                /* 80 */ ADDB,    ADDC,    ADDD,    ADDE,    ADDH,    ADDL,    ADDHL,    ADDA,    ADCB,    ADCC,    ADCD,    ADCE,    ADCH,    ADCL,    ADCHL,    ADCA,
                /* 90 */ SUBB,    SUBC,    SUBD,    SUBE,    SUBH,    SUBL,    SUBHL,    SUBA,    SBCB,    SBCC,    SBCD,    SBCE,    SBCH,    SBCL,    SBCHL,    SBCA,
                /* A0 */ ANDB,    ANDC,    ANDD,    ANDE,    ANDH,    ANDL,    ANDHL,    ANDA,    XORB,    XORC,    XORD,    XORE,    XORH,    XORL,    XORHL,    XORA,
                /* B0 */ ORB,     ORC,     ORD,     ORE,     ORH,     ORL,     ORHL,     ORA,     CPB,     CPC,     CPD,     CPE,     CPH,     CPL,     CPHL,     CPA,
                /* C0 */ RETNZ,   POPBC,   JPNZ,    JP,      CALLNZ,  PUSHBC,  ADDN,     RST00,   RETZ,    RET,     JPZ,     CB,      CALLZ,   CALL,    ADCN,     RST08,
                /* D0 */ RETNC,   POPDE,   JPNC,    NA,      CALLNC,  PUSHDE,  SUBN,     RST10,   RETC,    RETI,    JPC,     NA,      CALLC,   NA,      SBCN,     RST18,
                /* E0 */ LDIONA,  POPHL,   LDIOCA,  NA,      NA,      PUSHHL,  ANDN,     RST20,   ADDSPN,  JPHL,    LDNA,    NA,      NA,      NA,      XORN,     RST28,
                /* F0 */ LDIOAN,  POPAF,   LDIOAC,  DI,      NA,      PUSHAF,  ORN,      RST30,   LDHLSPN, LDSPHL,  LDANN,   EI,      NA,      NA,      CPN,      RST38
            };

            _cbOps = new List<Action>
            {
                /* 00 */ CBRLCB,  CBRLCC,  CBRLCD,  CBRLCE,  CBRLCH,  CBRLCL,  CBRLCHL,  CBRLCA,  CBRRCB,  CBRRCC,  CBRRCD,  CBRRCE,  CBRRCH,  CBRRCL,  CBRRCHL,  CBRRCA,
                /* 10 */ CBRLB,   CBRLC,   CBRLD,   CBRLE,   CBRLH,   CBRLL,   CBRLHL,   CBRLA,   CBRRB,   CBRRC,   CBRRD,   CBRRE,   CBRRH,   CBRRL,   CBRRHL,   CBRRA,
                /* 20 */ CBSLAB,  CBSLAC,  CBSLAD,  CBSLAE,  CBSLAH,  CBSLAL,  CBSLAHL,  CBSLAA,  CBSRAB,  CBSRAC,  CBSRAD,  CBSRAE,  CBSRAH,  CBSRAL,  CBSRAHL,  CBSRAA,
                /* 30 */ CBSWAPB, CBSWAPC, CBSWAPD, CBSWAPE, CBSWAPH, CBSWAPL, CBSWAPHL, CBSWAPA, CBSRLB,  CBSRLC,  CBSRLD,  CBSRLE,  CBSRLH,  CBSRLL,  CBSRLHL,  CBSRLA,
                /* 40 */ CBBIT0B, CBBIT0C, CBBIT0D, CBBIT0E, CBBIT0H, CBBIT0L, CBBIT0HL, CBBIT0A, CBBIT1B, CBBIT1C, CBBIT1D, CBBIT1E, CBBIT1H, CBBIT1L, CBBIT1HL, CBBIT1A,
                /* 50 */ CBBIT2B, CBBIT2C, CBBIT2D, CBBIT2E, CBBIT2H, CBBIT2L, CBBIT2HL, CBBIT2A, CBBIT3B, CBBIT3C, CBBIT3D, CBBIT3E, CBBIT3H, CBBIT3L, CBBIT3HL, CBBIT3A,
                /* 60 */ CBBIT4B, CBBIT4C, CBBIT4D, CBBIT4E, CBBIT4H, CBBIT4L, CBBIT4HL, CBBIT4A, CBBIT5B, CBBIT5C, CBBIT5D, CBBIT5E, CBBIT5H, CBBIT5L, CBBIT5HL, CBBIT5A,
                /* 70 */ CBBIT6B, CBBIT6C, CBBIT6D, CBBIT6E, CBBIT6H, CBBIT6L, CBBIT6HL, CBBIT6A, CBBIT7B, CBBIT7C, CBBIT7D, CBBIT7E, CBBIT7H, CBBIT7L, CBBIT7HL, CBBIT7A,
                /* 80 */ CBRES0B, CBRES0C, CBRES0D, CBRES0E, CBRES0H, CBRES0L, CBRES0HL, CBRES0A, CBRES1B, CBRES1C, CBRES1D, CBRES1E, CBRES1H, CBRES1L, CBRES1HL, CBRES1A,
                /* 90 */ CBRES2B, CBRES2C, CBRES2D, CBRES2E, CBRES2H, CBRES2L, CBRES2HL, CBRES2A, CBRES3B, CBRES3C, CBRES3D, CBRES3E, CBRES3H, CBRES3L, CBRES3HL, CBRES3A,
                /* A0 */ CBRES4B, CBRES4C, CBRES4D, CBRES4E, CBRES4H, CBRES4L, CBRES4HL, CBRES4A, CBRES5B, CBRES5C, CBRES5D, CBRES5E, CBRES5H, CBRES5L, CBRES5HL, CBRES5A,
                /* B0 */ CBRES6B, CBRES6C, CBRES6D, CBRES6E, CBRES6H, CBRES6L, CBRES6HL, CBRES6A, CBRES7B, CBRES7C, CBRES7D, CBRES7E, CBRES7H, CBRES7L, CBRES7HL, CBRES7A,
                /* C0 */ CBSET0B, CBSET0C, CBSET0D, CBSET0E, CBSET0H, CBSET0L, CBSET0HL, CBSET0A, CBSET1B, CBSET1C, CBSET1D, CBSET1E, CBSET1H, CBSET1L, CBSET1HL, CBSET1A,
                /* D0 */ CBSET2B, CBSET2C, CBSET2D, CBSET2E, CBSET2H, CBSET2L, CBSET2HL, CBSET2A, CBSET3B, CBSET3C, CBSET3D, CBSET3E, CBSET3H, CBSET3L, CBSET3HL, CBSET3A,
                /* E0 */ CBSET4B, CBSET4C, CBSET4D, CBSET4E, CBSET4H, CBSET4L, CBSET4HL, CBSET4A, CBSET5B, CBSET5C, CBSET5D, CBSET5E, CBSET5H, CBSET5L, CBSET5HL, CBSET5A,
                /* F0 */ CBSET6B, CBSET6C, CBSET6D, CBSET6E, CBSET6H, CBSET6L, CBSET6HL, CBSET6A, CBSET7B, CBSET7C, CBSET7D, CBSET7E, CBSET7H, CBSET7L, CBSET7HL, CBSET7A
            };
        }
    }
}
