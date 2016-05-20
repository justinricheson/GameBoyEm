using static GameBoyEm.CpuCycles;

namespace GameBoyEm
{
    public partial class Cpu
    {
		// Misc
        private void NOP() { }
        private void STOP() { }
        private void CB() { Step(_cbOps, CBCycles); }
        private void DAA()
        {
            // Adjust A into binary coded decimal form
            int a = A;
            if (FN)
            {
                if (FH || (a & _u4) > 9) // 9 is max BCD digit
                {
                    a += 6; // Overflow lo nibble
                }
                if (FC || a > 159) // Check if hi nibble exceeds 9
                {
                    a += 96; // Overflow hi nibble
                }
            }
            else
            {
                if (FH)
                {
                    a -= 6; // Underflow lo nibble
                    a &= _u8;
                }
                if (FC)
                {
                    a -= 96; // Underflow hi nibble
                }
            }

            FH = false;
            FC = (a & 256) == 256;
            A = (byte)a;
            FZ = A == 0;
        }

		// Complements and Flags
        private void SCF() { FC = true; FH = FN = false; }
        private void CPL() { A = A.XOR(_u8); FH = true; FN = true; }
		private void CCF() { FC = !FC; FH = false; FN = false; }

        // 8-bit Loads
        private void LDBA() { B = A; }
        private void LDBB() { } // NOP
        private void LDBC() { B = C; }
        private void LDBD() { B = D; }
        private void LDBE() { B = E; }
        private void LDBH() { B = H; }
        private void LDBL() { B = L; }
        private void LDCA() { C = A; }
        private void LDCB() { C = B; }
        private void LDCC() { } // NOP
        private void LDCD() { C = D; }
        private void LDCE() { C = E; }
        private void LDCH() { C = H; }
        private void LDCL() { C = L; }
        private void LDBHL() { B = RB(HL); }
        private void LDCHL() { C = RB(HL); }
		private void LDAN() { A = RB(PC++); }
        private void LDBN() { B = RB(PC++); }
        private void LDCN() { C = RB(PC++); }
        private void LDDN() { D = RB(PC++); }
        private void LDEN() { E = RB(PC++); }
        private void LDHN() { H = RB(PC++); }
        private void LDLN() { L = RB(PC++); }
        private void LDABC() { A = RB(BC); }
        private void LDADE() { A = RB(DE); }
        private void LDBCA() { WB(BC, A); }
        private void LDDEA() { WB(DE, A); }
        private void LDHLMN() { WB(HL, RB(PC++)); }
        private void LDIHLA() { WB(HL++, A); }
        private void LDDHLA() { WB(HL--, A); }
        private void LDIAHL() { A = RB(HL++); }
        private void LDDAHL() { A = RB(HL--); }

        // 8-bit Arithmetic
        private void INCA() { FH = (A & _u4) == _u4; A++; FN = false; FZ = A == 0; }
        private void INCB() { FH = (B & _u4) == _u4; B++; FN = false; FZ = B == 0; }
        private void INCC() { FH = (C & _u4) == _u4; C++; FN = false; FZ = C == 0; }
        private void INCD() { FH = (D & _u4) == _u4; D++; FN = false; FZ = D == 0; }
        private void INCE() { FH = (E & _u4) == _u4; E++; FN = false; FZ = E == 0; }
        private void INCH() { FH = (H & _u4) == _u4; H++; FN = false; FZ = H == 0; }
        private void INCL() { FH = (L & _u4) == _u4; L++; FN = false; FZ = L == 0; }
        private void DECA() { A--; FH = (A & _u4) != _u4; FN = true; FZ = A == 0; }
        private void DECB() { B--; FH = (B & _u4) != _u4; FN = true; FZ = B == 0; }
        private void DECC() { C--; FH = (C & _u4) != _u4; FN = true; FZ = C == 0; }
        private void DECD() { D--; FH = (D & _u4) != _u4; FN = true; FZ = D == 0; }
        private void DECE() { E--; FH = (E & _u4) != _u4; FN = true; FZ = E == 0; }
        private void DECH() { H--; FH = (H & _u4) != _u4; FN = true; FZ = H == 0; }
        private void DECL() { L--; FH = (L & _u4) != _u4; FN = true; FZ = L == 0; }
        private void INCHLM() { var n = RB(HL); FH = (n & _u4) == _u4; WB(HL, ++n); FN = false; FZ = n == 0; }
        private void DECHLM() { var n = RB(HL); WB(HL, --n); FH = (n & _u4) != _u4; FN = true; FZ = n == 0; }

        // 16-bit Loads
        private void LDBCN() { C = RB(PC++); B = RB(PC++); }
        private void LDDEN() { E = RB(PC++); D = RB(PC++); }
        private void LDNSP() { WW(RW(PC), SP); PC += 2; }
        private void LDHLN() { L = RB(PC++); H = RB(PC++); }
        private void LDSPN() { SP = RW(PC); PC += 2; }

        // 16-bit Arithmetic
        private void INCSP() { SP++; }
        private void DECSP() { SP--; }
        private void INCBC() { C++; if (C == 0) B++; }
        private void INCDE() { E++; if (E == 0) D++; }
        private void INCHL() { L++; if (L == 0) H++; }
        private void DECBC() { C--; if (C == _u8) B--; }
        private void DECDE() { E--; if (E == _u8) D--; }
        private void DECHL() { L--; if (L == _u8) H--; }
        private void ADDHLBC() { var hl = HL; var bc = BC; FH = (hl & _u12) + (bc & _u12) > _u12; int add = hl + bc; HL = (ushort)add; FC = add > _u16; FN = false; }
        private void ADDHLDE() { var hl = HL; var de = DE; FH = (hl & _u12) + (de & _u12) > _u12; int add = hl + de; HL = (ushort)add; FC = add > _u16; FN = false; }
        private void ADDHLHL() { var hl = HL; var hl12 = hl & _u12; FH = hl12 + hl12 > _u12; int add = hl + hl; HL = (ushort)add; FC = add > _u16; FN = false; }
        private void ADDHLSP() { var hl = HL; var sp = SP; FH = (hl & _u12) + (sp & _u12) > _u12; int add = hl + sp; HL = (ushort)add; FC = add > _u16; FN = false; }

        // Rotates and Shifts
        private void RRA() { var lo = A.AND(1); A = A.RS(1).OR(FC.LS(7)); F = 0; FC = lo == 1; FZ = A == 0; }
        private void RLA() { var hi = A.RS(7); A = A.LS(1).OR(FC); F = 0; FC = hi == 1; FZ = A == 0; }
        private void RRCA() { var lo = A.LS(7); A = lo.OR(A.RS()); F = 0; FC = lo == 1; FZ = A == 0; }
        private void RLCA() { var hi = A.RS(7); A = hi.OR(A.LS()); F = 0; FC = hi == 1; FZ = A == 0; }
        private void SRLB() { var lo = B.AND(1); B = B.RS(); F = 0; FC = lo == 1; FZ = B == 0; }

        // Jumps
        private void JRN() { Jump(); }
        private void JRZN() { Jump(FZ); }
        private void JRNZN() { Jump(!FZ); }
        private void JRNCN() { Jump(!FC); }

        // MMU Helpers
        private byte RB(ushort address)
        {
            return _mmu.ReadByte(address);
        }
        private ushort RW(ushort address)
        {
            return _mmu.ReadWord(address);
        }
        private void WB(ushort address, byte value)
        {
            _mmu.WriteByte(address, value);
        }
        private void WW(ushort address, ushort value)
        {
            _mmu.WriteWord(address, value);
        }

        // Jump Helpers
        private void Jump(bool predicate = true)
        {
            if (predicate)
            {
                var j = (sbyte)RB(PC++);
                var pc = (int)PC;
                pc += j;
                PC = (ushort)pc;
            }
        }
    }
}
