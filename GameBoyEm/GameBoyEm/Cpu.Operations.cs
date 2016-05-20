using System;
using static GameBoyEm.CpuCycles;

namespace GameBoyEm
{
    public partial class Cpu
    {
        // Misc
        private void NOP() { }
        private void HALT() { }
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
        private void LDAA() { } // NOP
        private void LDAB() { A = B; }
        private void LDAC() { A = C; }
        private void LDAD() { A = D; }
        private void LDAE() { A = E; }
        private void LDAH() { A = H; }
        private void LDAL() { A = L; }
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
        private void LDDA() { D = A; }
        private void LDDB() { D = B; }
        private void LDDC() { D = C; }
        private void LDDD() { } // NOP
        private void LDDE() { D = E; }
        private void LDDH() { D = H; }
        private void LDDL() { D = L; }
        private void LDEA() { E = A; }
        private void LDEB() { E = B; }
        private void LDEC() { E = C; }
        private void LDED() { E = D; }
        private void LDEE() { } // NOP
        private void LDEH() { E = H; }
        private void LDEL() { E = L; }
        private void LDHA() { H = A; }
        private void LDHB() { H = B; }
        private void LDHC() { H = C; }
        private void LDHD() { H = D; }
        private void LDHE() { H = E; }
        private void LDHH() { } // NOP
        private void LDHL() { H = L; }
        private void LDLA() { L = A; }
        private void LDLB() { L = B; }
        private void LDLC() { L = C; }
        private void LDLD() { L = D; }
        private void LDLE() { L = E; }
        private void LDLH() { L = H; }
        private void LDLL() { } // NOP
        private void LDAHL() { A = RB(HL); }
        private void LDBHL() { B = RB(HL); }
        private void LDCHL() { C = RB(HL); }
        private void LDDHL() { D = RB(HL); }
        private void LDEHL() { E = RB(HL); }
        private void LDHHL() { H = RB(HL); }
        private void LDLHL() { L = RB(HL); }
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
        private void LDHLA() { WB(HL, A); }
        private void LDHLB() { WB(HL, B); }
        private void LDHLC() { WB(HL, C); }
        private void LDHLD() { WB(HL, D); }
        private void LDHLE() { WB(HL, E); }
        private void LDHLH() { WB(HL, H); }
        private void LDHLL() { WB(HL, L); }
        private void LDHLMN() { WB(HL, RB(PC++)); }
        private void LDIHLA() { WB(HL++, A); }
        private void LDDHLA() { WB(HL--, A); }
        private void LDIAHL() { A = RB(HL++); }
        private void LDDAHL() { A = RB(HL--); }

        // 8-bit Arithmetic
        private void INCA() { Inc(ref _a); }
        private void INCB() { Inc(ref _b); }
        private void INCC() { Inc(ref _c); }
        private void INCD() { Inc(ref _d); }
        private void INCE() { Inc(ref _e); }
        private void INCH() { Inc(ref _h); }
        private void INCL() { Inc(ref _l); }
        private void DECA() { Dec(ref _a); }
        private void DECB() { Dec(ref _b); }
        private void DECC() { Dec(ref _c); }
        private void DECD() { Dec(ref _d); }
        private void DECE() { Dec(ref _e); }
        private void DECH() { Dec(ref _h); }
        private void DECL() { Dec(ref _l); }
        private void ADDA() { Add(A); }
        private void ADDB() { Add(B); }
        private void ADDC() { Add(C); }
        private void ADDD() { Add(D); }
        private void ADDE() { Add(E); }
        private void ADDH() { Add(H); }
        private void ADDL() { Add(L); }
        private void ADDHL() { Add(RB(HL)); }
        private void ADCA() { AddC(A); }
        private void ADCB() { AddC(B); }
        private void ADCC() { AddC(C); }
        private void ADCD() { AddC(D); }
        private void ADCE() { AddC(E); }
        private void ADCH() { AddC(H); }
        private void ADCL() { AddC(L); }
        private void SUBA() { Sub(A); }
        private void SUBB() { Sub(B); }
        private void SUBC() { Sub(C); }
        private void SUBD() { Sub(D); }
        private void SUBE() { Sub(E); }
        private void SUBH() { Sub(H); }
        private void SUBL() { Sub(L); }
        private void SUBHL() { Sub(RB(HL)); }
        private void SBCA() { SubC(A); }
        private void SBCB() { SubC(B); }
        private void SBCC() { SubC(C); }
        private void SBCD() { SubC(D); }
        private void SBCE() { SubC(E); }
        private void SBCH() { SubC(H); }
        private void SBCL() { SubC(L); }
        private void SBCHL() { SubC(RB(HL)); }
        private void ANDA() { And(A); }
        private void ANDB() { And(B); }
        private void ANDC() { And(C); }
        private void ANDD() { And(D); }
        private void ANDE() { And(E); }
        private void ANDH() { And(H); }
        private void ANDL() { And(L); }
        private void ANDHL() { And(RB(HL)); }
        private void XORA() { Xor(A); }
        private void XORB() { Xor(B); }
        private void XORC() { Xor(C); }
        private void XORD() { Xor(D); }
        private void XORE() { Xor(E); }
        private void XORH() { Xor(H); }
        private void XORL() { Xor(L); }
        private void XORHL() { Xor(RB(HL)); }

        private void ADCHL() { AddC(RB(HL)); }
        private void INCHLM() { var n = RB(HL); FH = (n & _u4) == _u4; WB(HL, ++n); FN = false; FZ = n == 0; }
        private void DECHLM() { var n = RB(HL); WB(HL, --n); FH = (n & _u4) != _u4; FN = true; FZ = n == 0; }

        // 16-bit Loads
        private void LDBCN() { C = RB(PC++); B = RB(PC++); }
        private void LDDEN() { E = RB(PC++); D = RB(PC++); }
        private void LDHLN() { L = RB(PC++); H = RB(PC++); }
        private void LDSPN() { SP = RW(PC); PC += 2; }
        private void LDNSP() { WW(RW(PC), SP); PC += 2; }

        // 16-bit Arithmetic
        private void INCSP() { SP++; }
        private void DECSP() { SP--; }
        private void INCBC() { C++; if (C == 0) B++; }
        private void INCDE() { E++; if (E == 0) D++; }
        private void INCHL() { L++; if (L == 0) H++; }
        private void DECBC() { C--; if (C == _u8) B--; }
        private void DECDE() { E--; if (E == _u8) D--; }
        private void DECHL() { L--; if (L == _u8) H--; }
        private void ADDHLBC() { Add16(BC); }
        private void ADDHLDE() { Add16(DE); }
        private void ADDHLHL() { Add16(HL); }
        private void ADDHLSP() { Add16(SP); }

        // Rotates and Shifts
        private void RRA() { var lo = A.AND(1); A = A.RS().OR(FC.LS(7)); F = 0; FC = lo == 1; FZ = A == 0; }
        private void RLA() { var hi = A.AND(128); A = A.LS().OR(FC); F = 0; FC = hi == 128; FZ = A == 0; }
        private void RRCA() { var lo = A.AND(1).LS(7); A = A.RS().OR(lo); F = 0; FC = lo == 128; FZ = A == 0; }
        private void RLCA() { var hi = A.AND(128).RS(7); A = A.LS().OR(hi); F = 0; FC = hi == 1; FZ = A == 0; }
        private void SRLB() { var lo = B.AND(1); B = B.RS(); F = 0; FC = lo == 1; FZ = B == 0; }

        // Jumps
        private void JRN() { Jump(); }
        private void JRZN() { Jump(FZ); }
        private void JRNZN() { Jump(!FZ); }
        private void JRNCN() { Jump(!FC); }

        // Helpers
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
        private void Inc(ref byte register)
        {
            FH = (register & _u4) == _u4;
            register++;
            FN = false;
            FZ = register == 0;
        }
        private void Dec(ref byte register)
        {
            register--;

            // CPU manual says half carry set on NO borrow
            //FH = (register & _u4) != _u4;

            // Ref impl says half carry set on borrow
            FH = (register & _u4) == _u4;

            FN = true;
            FZ = register == 0;
        }
        private void Add(byte register)
        {
            var a = A;
            var ar = a + register;
            A = (byte)ar;
            F = 0;
            FC = ar > _u8;
            FH = ((a & _u4) + (register & _u4) & 16) != 0;
            FZ = A == 0;
        }
        private void AddC(byte register)
        {
            var a = A;
            var carry = FC ? 1 : 0;
            var ar = a + register + carry;
            A = (byte)(ar);
            F = 0;
            FC = ar > _u8;
            FH = ((a & _u4) + (register & _u4) + carry & 16) != 0;
            FZ = A == 0;
        }
        private void Add16(ushort register)
        {
            var hl = HL;
            FH = (hl & _u12) + (register & _u12) > _u12;
            int add = hl + register;
            HL = (ushort)add;
            FC = add > _u16;
            FN = false;
        }
        private void Sub(byte register)
        {
            var a = A;
            A -= register;

            // CPU manual says carry set on NO borrow
            //FC = a - register >= 0;

            // Ref impl says carry set on borrow
            FC = a - register < 0;

            // CPU manual says half carry set on NO borrow
            //FH = ((A ^ register ^ a) & 16) == 0;

            // Ref impl says half carry set on borrow
            FH = ((A ^ register ^ a) & 16) > 0;

            FN = true;
            FZ = A == 0;
        }
        private void SubC(byte register)
        {
            var a = A;
            var carry = FC ? 1 : 0;
            var ar = A - register - carry;
            A = (byte)ar;

            // CPU manual says carry set on NO borrow
            //FC = ar >= 0;

            // Ref impl says carry set on borrow
            FC = ar < 0;

            // CPU manual says half carry set on NO borrow
            //FH = ((a & _u4) - (register & _u4) - carry) >= 0;

            // Ref impl says half carry set on borrow
            FH = ((a & _u4) - (register & _u4) - carry) < 0;

            FN = true;
            FZ = A == 0;
        }
        private void And(byte register)
        {
            A &= register;
            FC = false;
            FH = true;
            FN = false;
            FZ = A == 0;
        }
        private void Xor(byte register)
        {
            A ^= register;
            FC = false;
            FH = false;
            FN = false;
            FZ = A == 0;
        }
    }
}
