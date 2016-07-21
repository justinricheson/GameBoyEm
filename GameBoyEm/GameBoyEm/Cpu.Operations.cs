using static GameBoyEm.CpuCycles;

namespace GameBoyEm
{
    public partial class Cpu
    {
        // Misc
        private void NA() { } // Stub for invalid opcodes
        private void DI() { IME = false; }
        private void EI() { IME = true; }
        private void NOP() { }
        private void HALT() { }
        private void STOP() { }
        private void CB() { Step(_cbOps, CBCycles); }
        private void DAA()
        {
            // Adjust A into binary coded decimal form
            int a = A;
            if (!FN)
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

            // Ref impl only sets FC if carry
            // Docs say to set or unset based on carry/no-carry
            FC = (a & 256) == 256;
            A = (byte)a;
            FZ = A == 0;
        }

        // Complements and Flags
        private void SCF() { FC = true; FH = FN = false; }
        private void CCF() { FC = !FC; FH = false; FN = false; }
        private void CMPL() { A = A.XOR(_u8); FH = true; FN = true; }

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
        private void LDAN() { A = RBN(); }
        private void LDBN() { B = RBN(); }
        private void LDCN() { C = RBN(); }
        private void LDDN() { D = RBN(); }
        private void LDEN() { E = RBN(); }
        private void LDHN() { H = RBN(); }
        private void LDLN() { L = RBN(); }
        private void LDANN() { A = RB(RWN()); }
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
        private void LDNA() { WB(RW(PC), A); PC += 2; }
        private void LDHLMN() { WB(HL, RBN()); }
        private void LDIHLA() { WB(HL++, A); }
        private void LDDHLA() { WB(HL--, A); }
        private void LDIAHL() { A = RB(HL++); }
        private void LDDAHL() { A = RB(HL--); }
        private void LDIOAC() { A = RB((ushort)(_io + C)); }
        private void LDIOCA() { WB((ushort)(_io + C), A); }
        private void LDIONA() { var n = RBN(); WB((ushort)(_io + n), A); }
        private void LDIOAN() { var n = RBN(); A = RB((ushort)(_io + n)); }

        // 8-bit Arithmetic
        private void INCA() { A = Inc(A); }
        private void INCB() { B = Inc(B); }
        private void INCC() { C = Inc(C); }
        private void INCD() { D = Inc(D); }
        private void INCE() { E = Inc(E); }
        private void INCH() { H = Inc(H); }
        private void INCL() { L = Inc(L); }
        private void DECA() { A = Dec(A); }
        private void DECB() { B = Dec(B); }
        private void DECC() { C = Dec(C); }
        private void DECD() { D = Dec(D); }
        private void DECE() { E = Dec(E); }
        private void DECH() { H = Dec(H); }
        private void DECL() { L = Dec(L); }
        private void ADDA() { Add(A); }
        private void ADDB() { Add(B); }
        private void ADDC() { Add(C); }
        private void ADDD() { Add(D); }
        private void ADDE() { Add(E); }
        private void ADDH() { Add(H); }
        private void ADDL() { Add(L); }
        private void ADDHL() { Add(RB(HL)); }
        private void ADDN() { Add(RBN()); }
        private void ADCA() { AddC(A); }
        private void ADCB() { AddC(B); }
        private void ADCC() { AddC(C); }
        private void ADCD() { AddC(D); }
        private void ADCE() { AddC(E); }
        private void ADCH() { AddC(H); }
        private void ADCL() { AddC(L); }
        private void ADCHL() { AddC(RB(HL)); }
        private void ADCN() { AddC(RBN()); }
        private void SUBA() { Sub(A); }
        private void SUBB() { Sub(B); }
        private void SUBC() { Sub(C); }
        private void SUBD() { Sub(D); }
        private void SUBE() { Sub(E); }
        private void SUBH() { Sub(H); }
        private void SUBL() { Sub(L); }
        private void SUBHL() { Sub(RB(HL)); }
        private void SUBN() { Sub(RBN()); }
        private void SBCA() { SubC(A); }
        private void SBCB() { SubC(B); }
        private void SBCC() { SubC(C); }
        private void SBCD() { SubC(D); }
        private void SBCE() { SubC(E); }
        private void SBCH() { SubC(H); }
        private void SBCL() { SubC(L); }
        private void SBCHL() { SubC(RB(HL)); }
        private void SBCN() { SubC(RBN()); }
        private void ANDA() { And(A); }
        private void ANDB() { And(B); }
        private void ANDC() { And(C); }
        private void ANDD() { And(D); }
        private void ANDE() { And(E); }
        private void ANDH() { And(H); }
        private void ANDL() { And(L); }
        private void ANDHL() { And(RB(HL)); }
        private void ANDN() { And(RBN()); }
        private void XORA() { Xor(A); }
        private void XORB() { Xor(B); }
        private void XORC() { Xor(C); }
        private void XORD() { Xor(D); }
        private void XORE() { Xor(E); }
        private void XORH() { Xor(H); }
        private void XORL() { Xor(L); }
        private void XORHL() { Xor(RB(HL)); }
        private void XORN() { Xor(RBN()); }
        private void ORA() { Or(A); }
        private void ORB() { Or(B); }
        private void ORC() { Or(C); }
        private void ORD() { Or(D); }
        private void ORE() { Or(E); }
        private void ORH() { Or(H); }
        private void ORL() { Or(L); }
        private void ORHL() { Or(RB(HL)); }
        private void ORN() { Or(RBN()); }
        private void CPA() { Compare(A); }
        private void CPB() { Compare(B); }
        private void CPC() { Compare(C); }
        private void CPD() { Compare(D); }
        private void CPE() { Compare(E); }
        private void CPH() { Compare(H); }
        private void CPL() { Compare(L); }
        private void CPHL() { Compare(RB(HL)); }
        private void CPN() { Compare(RBN()); }
        private void INCHLM() { var n = (byte)(RB(HL) + 1); WB(HL, n); FH = (n & _u4) == 0; FN = false; FZ = n == 0; }
        private void DECHLM() { var n = (byte)(RB(HL) - 1); WB(HL, n); FH = (n & _u4) == _u4; FN = true; FZ = n == 0; }

        // 16-bit Loads
        private void LDBCN() { BC = RWN(); }
        private void LDDEN() { DE = RWN(); }
        private void LDHLN() { HL = RWN(); }
        private void LDSPN() { SP = RWN(); }
        private void LDNSP() { WW(RWN(), SP); ; }
        private void LDHLSPN() { HL = Add16_2((sbyte)RBN(), SP); }
        private void LDSPHL() { SP = HL; }

        // 16-bit Arithmetic
        private void INCSP() { SP++; }
        private void DECSP() { SP--; }
        private void INCBC() { C++; if (C == 0) B++; }
        private void INCDE() { E++; if (E == 0) D++; }
        private void INCHL() { L++; if (L == 0) H++; }
        private void DECBC() { C--; if (C == _u8) B--; }
        private void DECDE() { E--; if (E == _u8) D--; }
        private void DECHL() { L--; if (L == _u8) H--; }
        private void ADDHLBC() { HL = Add16(BC, HL); }
        private void ADDHLDE() { HL = Add16(DE, HL); }
        private void ADDHLHL() { HL = Add16(HL, HL); }
        private void ADDHLSP() { HL = Add16(SP, HL); }
        private void ADDSPN() { SP = Add16_2((sbyte)RBN(), SP); }

        // Rotates, Shifts, and Swaps
        private void RRA() { A = RotateRight(A); }
        private void RLA() { A = RotateLeft(A); }
        private void RRCA() { A = RotateRightC(A, true); }
        private void RLCA() { A = RotateLeftC(A, true); }
        private void CBRRCA() { A = RotateRightC(A); }
        private void CBRRCB() { B = RotateRightC(B); }
        private void CBRRCC() { C = RotateRightC(C); }
        private void CBRRCD() { D = RotateRightC(D); }
        private void CBRRCE() { E = RotateRightC(E); }
        private void CBRRCH() { H = RotateRightC(H); }
        private void CBRRCL() { L = RotateRightC(L); }
        private void CBRRCHL() { WB(HL, RotateRightC(RB(HL))); }
        private void CBRLCA() { A = RotateLeftC(A); }
        private void CBRLCB() { B = RotateLeftC(B); }
        private void CBRLCC() { C = RotateLeftC(C); }
        private void CBRLCD() { D = RotateLeftC(D); }
        private void CBRLCE() { E = RotateLeftC(E); }
        private void CBRLCH() { H = RotateLeftC(H); }
        private void CBRLCL() { L = RotateLeftC(L); }
        private void CBRLCHL() { WB(HL, RotateLeftC(RB(HL))); }
        private void CBRRA() { A = RotateRight(A); }
        private void CBRRB() { B = RotateRight(B); }
        private void CBRRC() { C = RotateRight(C); }
        private void CBRRD() { D = RotateRight(D); }
        private void CBRRE() { E = RotateRight(E); }
        private void CBRRH() { H = RotateRight(H); }
        private void CBRRL() { L = RotateRight(L); }
        private void CBRRHL() { WB(HL, RotateRight(RB(HL))); }
        private void CBRLA() { A = RotateLeft(A); }
        private void CBRLB() { B = RotateLeft(B); }
        private void CBRLC() { C = RotateLeft(C); }
        private void CBRLD() { D = RotateLeft(D); }
        private void CBRLE() { E = RotateLeft(E); }
        private void CBRLH() { H = RotateLeft(H); }
        private void CBRLL() { L = RotateLeft(L); }
        private void CBRLHL() { WB(HL, RotateLeft(RB(HL))); }
        private void CBSRAA() { A = ShiftRightMsb(A); }
        private void CBSRAB() { B = ShiftRightMsb(B); }
        private void CBSRAC() { C = ShiftRightMsb(C); }
        private void CBSRAD() { D = ShiftRightMsb(D); }
        private void CBSRAE() { E = ShiftRightMsb(E); }
        private void CBSRAH() { H = ShiftRightMsb(H); }
        private void CBSRAL() { L = ShiftRightMsb(L); }
        private void CBSRAHL() { WB(HL, ShiftRightMsb(RB(HL))); }
        private void CBSLAA() { A = ShiftLeft(A); }
        private void CBSLAB() { B = ShiftLeft(B); }
        private void CBSLAC() { C = ShiftLeft(C); }
        private void CBSLAD() { D = ShiftLeft(D); }
        private void CBSLAE() { E = ShiftLeft(E); }
        private void CBSLAH() { H = ShiftLeft(H); }
        private void CBSLAL() { L = ShiftLeft(L); }
        private void CBSLAHL() { WB(HL, ShiftLeft(RB(HL))); }
        private void CBSWAPA() { A = Swap(A); }
        private void CBSWAPB() { B = Swap(B); }
        private void CBSWAPC() { C = Swap(C); }
        private void CBSWAPD() { D = Swap(D); }
        private void CBSWAPE() { E = Swap(E); }
        private void CBSWAPH() { H = Swap(H); }
        private void CBSWAPL() { L = Swap(L); }
        private void CBSWAPHL() { WB(HL, Swap(RB(HL))); }
        private void CBSRLA() { A = ShiftRight(A); }
        private void CBSRLB() { B = ShiftRight(B); }
        private void CBSRLC() { C = ShiftRight(C); }
        private void CBSRLD() { D = ShiftRight(D); }
        private void CBSRLE() { E = ShiftRight(E); }
        private void CBSRLH() { H = ShiftRight(H); }
        private void CBSRLL() { L = ShiftRight(L); }
        private void CBSRLHL() { WB(HL, ShiftRight(RB(HL))); }

        // Jumps, Calls, Returns, and Resets
        private void JR() { JumpRel(); }
        private void JRZ() { JumpRel(FZ); }
        private void JRC() { JumpRel(FC); }
        private void JRNZ() { JumpRel(!FZ); }
        private void JRNC() { JumpRel(!FC); }
        private void JP() { JumpAbs(); }
        private void JPZ() { JumpAbs(FZ); }
        private void JPC() { JumpAbs(FC); }
        private void JPNZ() { JumpAbs(!FZ); }
        private void JPNC() { JumpAbs(!FC); }
        private void JPHL() { PC = HL; }
        private void RET() { Return(); }
        private void RETZ() { Return(FZ); }
        private void RETC() { Return(FC); }
        private void RETNZ() { Return(!FZ); }
        private void RETNC() { Return(!FC); }
        private void RETI() { Return(); IME = true; }
        private void CALL() { Call(); }
        private void CALLZ() { Call(FZ); }
        private void CALLC() { Call(FC); }
        private void CALLNZ() { Call(!FZ); }
        private void CALLNC() { Call(!FC); }
        private void RST00() { Reset(0); }
        private void RST08() { Reset(8); }
        private void RST10() { Reset(16); }
        private void RST18() { Reset(24); }
        private void RST20() { Reset(32); }
        private void RST28() { Reset(40); }
        private void RST30() { Reset(48); }
        private void RST38() { Reset(56); }

        // Push and Pop
        private void POPAF() { AF = Pop(); }
        private void POPBC() { BC = Pop(); }
        private void POPDE() { DE = Pop(); }
        private void POPHL() { HL = Pop(); }
        private void PUSHAF() { Push(AF); }
        private void PUSHBC() { Push(BC); }
        private void PUSHDE() { Push(DE); }
        private void PUSHHL() { Push(HL); }

        // Bit Operations
        private void CBBIT0A() { GetBit(A, 1); }
        private void CBBIT0B() { GetBit(B, 1); }
        private void CBBIT0C() { GetBit(C, 1); }
        private void CBBIT0D() { GetBit(D, 1); }
        private void CBBIT0E() { GetBit(E, 1); }
        private void CBBIT0H() { GetBit(H, 1); }
        private void CBBIT0L() { GetBit(L, 1); }
        private void CBBIT0HL() { GetBit(RB(HL), 1); }
        private void CBBIT1A() { GetBit(A, 2); }
        private void CBBIT1B() { GetBit(B, 2); }
        private void CBBIT1C() { GetBit(C, 2); }
        private void CBBIT1D() { GetBit(D, 2); }
        private void CBBIT1E() { GetBit(E, 2); }
        private void CBBIT1H() { GetBit(H, 2); }
        private void CBBIT1L() { GetBit(L, 2); }
        private void CBBIT1HL() { GetBit(RB(HL), 2); }
        private void CBBIT2A() { GetBit(A, 4); }
        private void CBBIT2B() { GetBit(B, 4); }
        private void CBBIT2C() { GetBit(C, 4); }
        private void CBBIT2D() { GetBit(D, 4); }
        private void CBBIT2E() { GetBit(E, 4); }
        private void CBBIT2H() { GetBit(H, 4); }
        private void CBBIT2L() { GetBit(L, 4); }
        private void CBBIT2HL() { GetBit(RB(HL), 4); }
        private void CBBIT3A() { GetBit(A, 8); }
        private void CBBIT3B() { GetBit(B, 8); }
        private void CBBIT3C() { GetBit(C, 8); }
        private void CBBIT3D() { GetBit(D, 8); }
        private void CBBIT3E() { GetBit(E, 8); }
        private void CBBIT3H() { GetBit(H, 8); }
        private void CBBIT3L() { GetBit(L, 8); }
        private void CBBIT3HL() { GetBit(RB(HL), 8); }
        private void CBBIT4A() { GetBit(A, 16); }
        private void CBBIT4B() { GetBit(B, 16); }
        private void CBBIT4C() { GetBit(C, 16); }
        private void CBBIT4D() { GetBit(D, 16); }
        private void CBBIT4E() { GetBit(E, 16); }
        private void CBBIT4H() { GetBit(H, 16); }
        private void CBBIT4L() { GetBit(L, 16); }
        private void CBBIT4HL() { GetBit(RB(HL), 16); }
        private void CBBIT5A() { GetBit(A, 32); }
        private void CBBIT5B() { GetBit(B, 32); }
        private void CBBIT5C() { GetBit(C, 32); }
        private void CBBIT5D() { GetBit(D, 32); }
        private void CBBIT5E() { GetBit(E, 32); }
        private void CBBIT5H() { GetBit(H, 32); }
        private void CBBIT5L() { GetBit(L, 32); }
        private void CBBIT5HL() { GetBit(RB(HL), 32); }
        private void CBBIT6A() { GetBit(A, 64); }
        private void CBBIT6B() { GetBit(B, 64); }
        private void CBBIT6C() { GetBit(C, 64); }
        private void CBBIT6D() { GetBit(D, 64); }
        private void CBBIT6E() { GetBit(E, 64); }
        private void CBBIT6H() { GetBit(H, 64); }
        private void CBBIT6L() { GetBit(L, 64); }
        private void CBBIT6HL() { GetBit(RB(HL), 64); }
        private void CBBIT7A() { GetBit(A, 128); }
        private void CBBIT7B() { GetBit(B, 128); }
        private void CBBIT7C() { GetBit(C, 128); }
        private void CBBIT7D() { GetBit(D, 128); }
        private void CBBIT7E() { GetBit(E, 128); }
        private void CBBIT7H() { GetBit(H, 128); }
        private void CBBIT7L() { GetBit(L, 128); }
        private void CBBIT7HL() { GetBit(RB(HL), 128); }
        private void CBRES0A() { A &= 254; }
        private void CBRES0B() { B &= 254; }
        private void CBRES0C() { C &= 254; }
        private void CBRES0D() { D &= 254; }
        private void CBRES0E() { E &= 254; }
        private void CBRES0H() { H &= 254; }
        private void CBRES0L() { L &= 254; }
        private void CBRES0HL() { WB(HL, RB(HL).AND(254)); }
        private void CBRES1A() { A &= 253; }
        private void CBRES1B() { B &= 253; }
        private void CBRES1C() { C &= 253; }
        private void CBRES1D() { D &= 253; }
        private void CBRES1E() { E &= 253; }
        private void CBRES1H() { H &= 253; }
        private void CBRES1L() { L &= 253; }
        private void CBRES1HL() { WB(HL, RB(HL).AND(253)); }
        private void CBRES2A() { A &= 251; }
        private void CBRES2B() { B &= 251; }
        private void CBRES2C() { C &= 251; }
        private void CBRES2D() { D &= 251; }
        private void CBRES2E() { E &= 251; }
        private void CBRES2H() { H &= 251; }
        private void CBRES2L() { L &= 251; }
        private void CBRES2HL() { WB(HL, RB(HL).AND(251)); }
        private void CBRES3A() { A &= 247; }
        private void CBRES3B() { B &= 247; }
        private void CBRES3C() { C &= 247; }
        private void CBRES3D() { D &= 247; }
        private void CBRES3E() { E &= 247; }
        private void CBRES3H() { H &= 247; }
        private void CBRES3L() { L &= 247; }
        private void CBRES3HL() { WB(HL, RB(HL).AND(247)); }
        private void CBRES4A() { A &= 239; }
        private void CBRES4B() { B &= 239; }
        private void CBRES4C() { C &= 239; }
        private void CBRES4D() { D &= 239; }
        private void CBRES4E() { E &= 239; }
        private void CBRES4H() { H &= 239; }
        private void CBRES4L() { L &= 239; }
        private void CBRES4HL() { WB(HL, RB(HL).AND(239)); }
        private void CBRES5A() { A &= 223; }
        private void CBRES5B() { B &= 223; }
        private void CBRES5C() { C &= 223; }
        private void CBRES5D() { D &= 223; }
        private void CBRES5E() { E &= 223; }
        private void CBRES5H() { H &= 223; }
        private void CBRES5L() { L &= 223; }
        private void CBRES5HL() { WB(HL, RB(HL).AND(223)); }
        private void CBRES6A() { A &= 191; }
        private void CBRES6B() { B &= 191; }
        private void CBRES6C() { C &= 191; }
        private void CBRES6D() { D &= 191; }
        private void CBRES6E() { E &= 191; }
        private void CBRES6H() { H &= 191; }
        private void CBRES6L() { L &= 191; }
        private void CBRES6HL() { WB(HL, RB(HL).AND(191)); }
        private void CBRES7A() { A &= 127; }
        private void CBRES7B() { B &= 127; }
        private void CBRES7C() { C &= 127; }
        private void CBRES7D() { D &= 127; }
        private void CBRES7E() { E &= 127; }
        private void CBRES7H() { H &= 127; }
        private void CBRES7L() { L &= 127; }
        private void CBRES7HL() { WB(HL, RB(HL).AND(127)); }
        private void CBSET0A() { A |= 1; }
        private void CBSET0B() { B |= 1; }
        private void CBSET0C() { C |= 1; }
        private void CBSET0D() { D |= 1; }
        private void CBSET0E() { E |= 1; }
        private void CBSET0H() { H |= 1; }
        private void CBSET0L() { L |= 1; }
        private void CBSET0HL() { WB(HL, RB(HL).OR(1)); }
        private void CBSET1A() { A |= 2; }
        private void CBSET1B() { B |= 2; }
        private void CBSET1C() { C |= 2; }
        private void CBSET1D() { D |= 2; }
        private void CBSET1E() { E |= 2; }
        private void CBSET1H() { H |= 2; }
        private void CBSET1L() { L |= 2; }
        private void CBSET1HL() { WB(HL, RB(HL).OR(2)); }
        private void CBSET2A() { A |= 4; }
        private void CBSET2B() { B |= 4; }
        private void CBSET2C() { C |= 4; }
        private void CBSET2D() { D |= 4; }
        private void CBSET2E() { E |= 4; }
        private void CBSET2H() { H |= 4; }
        private void CBSET2L() { L |= 4; }
        private void CBSET2HL() { WB(HL, RB(HL).OR(4)); }
        private void CBSET3A() { A |= 8; }
        private void CBSET3B() { B |= 8; }
        private void CBSET3C() { C |= 8; }
        private void CBSET3D() { D |= 8; }
        private void CBSET3E() { E |= 8; }
        private void CBSET3H() { H |= 8; }
        private void CBSET3L() { L |= 8; }
        private void CBSET3HL() { WB(HL, RB(HL).OR(8)); }
        private void CBSET4A() { A |= 16; }
        private void CBSET4B() { B |= 16; }
        private void CBSET4C() { C |= 16; }
        private void CBSET4D() { D |= 16; }
        private void CBSET4E() { E |= 16; }
        private void CBSET4H() { H |= 16; }
        private void CBSET4L() { L |= 16; }
        private void CBSET4HL() { WB(HL, RB(HL).OR(16)); }
        private void CBSET5A() { A |= 32; }
        private void CBSET5B() { B |= 32; }
        private void CBSET5C() { C |= 32; }
        private void CBSET5D() { D |= 32; }
        private void CBSET5E() { E |= 32; }
        private void CBSET5H() { H |= 32; }
        private void CBSET5L() { L |= 32; }
        private void CBSET5HL() { WB(HL, RB(HL).OR(32)); }
        private void CBSET6A() { A |= 64; }
        private void CBSET6B() { B |= 64; }
        private void CBSET6C() { C |= 64; }
        private void CBSET6D() { D |= 64; }
        private void CBSET6E() { E |= 64; }
        private void CBSET6H() { H |= 64; }
        private void CBSET6L() { L |= 64; }
        private void CBSET6HL() { WB(HL, RB(HL).OR(64)); }
        private void CBSET7A() { A |= 128; }
        private void CBSET7B() { B |= 128; }
        private void CBSET7C() { C |= 128; }
        private void CBSET7D() { D |= 128; }
        private void CBSET7E() { E |= 128; }
        private void CBSET7H() { H |= 128; }
        private void CBSET7L() { L |= 128; }
        private void CBSET7HL() { WB(HL, RB(HL).OR(128)); }

        // Helpers
        private byte RB(ushort address)
        {
            return _mmu.ReadByte(address);
        }
        private byte RBN() => RB(PC++);
        private ushort RW(ushort address)
        {
            return _mmu.ReadWord(address);
        }
        private ushort RWN()
        {
            var w = RW(PC);
            PC += 2;
            return w;
        }
        private void WB(ushort address, byte value)
        {
            _mmu.WriteByte(address, value);
        }
        private void WW(ushort address, ushort value)
        {
            _mmu.WriteWord(address, value);
        }
        private void JumpRel(bool predicate = true)
        {
            var j = (sbyte)RB(PC++); // Always read the argument
            if (predicate)
            {
                var pc = (int)PC;
                pc += j;
                PC = (ushort)pc;
            }
        }
        private void JumpAbs(bool predicate = true)
        {
            if (predicate)
            {
                PC = RW(PC);
            }
            else
            {
                PC += 2; // Jump over argument
            }
        }
        private void Return(bool predicate = true)
        {
            if (predicate)
            {
                PC = RW(SP);
                SP += 2;
            }
        }
        private void Call(bool predicate = true)
        {
            if (predicate)
            {
                SP -= 2;
                WW(SP, (ushort)(PC + 2)); // Push return address onto stack
                PC = RW(PC); // Jump to argument
            }
            else
            {
                PC += 2; // Jump over argument
            }
        }
        private byte Inc(byte value)
        {
            FH = (value & _u4) == _u4;
            value++;
            FN = false;
            FZ = value == 0;

            return value;
        }
        private byte Dec(byte value)
        {
            value--;

            // CPU manual says half carry set on NO borrow
            //FH = (register & _u4) != _u4;

            // Ref impl says half carry set on borrow
            FH = (value & _u4) == _u4;

            FN = true;
            FZ = value == 0;

            return value;
        }
        private void Add(byte value)
        {
            var a = A;
            var ar = a + value;
            A = (byte)ar;
            F = 0;
            FC = ar > _u8;
            FH = ((a & _u4) + (value & _u4) & 16) != 0;
            FZ = A == 0;
        }
        private void AddC(byte value)
        {
            var a = A;
            var carry = FC ? 1 : 0;
            var ar = a + value + carry;
            A = (byte)(ar);
            F = 0;
            FC = ar > _u8;
            FH = ((a & _u4) + (value & _u4) + carry & 16) != 0;
            FZ = A == 0;
        }
        private ushort Add16(ushort x, ushort y)
        {
            int add = x + y;
            FC = add > _u16;
            FH = (x & _u12) + (y & _u12) > _u12;
            FN = false;
            return (ushort)add;
        }
        private ushort Add16_2(sbyte x, ushort y)
        {
            // Apparently ADDSPN and LDHLSPN are special
            // snowflakes that need to set the flags differently

            // These don't work with negative numbers.
            // Must set carry flags on borrows as well
            //FC = (x & _u12) + (y & _u12) > _u12;
            //FH = (x & _u4) + (y & _u4) > _u4;

            var r = (ushort)(x + y);

            // These work because xor is the same as adding without the carry/borrow
            // So if we xor the initial values, then xor with the original addition,
            // we get only the carried/borrowed bits. Then we just mask the result
            // to check if the carry/borrow is in the right spot
            FC = ((x ^ y ^ r) & 256) == 256;
            FH = ((x ^ y ^ r) & 16) == 16;
            FN = false;
            FZ = false;
            return r;
        }
        private void Sub(byte value)
        {
            var a = A;
            A -= value;

            // CPU manual says carry set on NO borrow
            //FC = a - register >= 0;

            // Ref impl says carry set on borrow
            FC = a - value < 0;

            // CPU manual says half carry set on NO borrow
            //FH = ((A ^ register ^ a) & 16) == 0;

            // Ref impl says half carry set on borrow
            FH = ((A ^ value ^ a) & 16) > 0;

            FN = true;
            FZ = A == 0;
        }
        private void SubC(byte value)
        {
            var a = A;
            var carry = FC ? 1 : 0;
            var ar = A - value - carry;
            A = (byte)ar;

            // CPU manual says carry set on NO borrow
            //FC = ar >= 0;

            // Ref impl says carry set on borrow
            FC = ar < 0;

            // CPU manual says half carry set on NO borrow
            //FH = ((a & _u4) - (register & _u4) - carry) >= 0;

            // Ref impl says half carry set on borrow
            FH = ((a & _u4) - (value & _u4) - carry) < 0;

            FN = true;
            FZ = A == 0;
        }
        private void And(byte value)
        {
            A &= value;
            F = 0;
            FH = true;
            FZ = A == 0;
        }
        private void Xor(byte value)
        {
            A ^= value;
            F = 0;
            FZ = A == 0;
        }
        private void Or(byte value)
        {
            A |= value;
            F = 0;
            FZ = A == 0;
        }
        private void Compare(byte value)
        {
            var r = A - value;
            FC = A < value;

            // CPU manual says half carry set on NO borrow
            //FH = ((r ^ register ^ A) & 16) == 0;

            // Ref impl says half carry set on borrow
            FH = ((r ^ value ^ A) & 16) > 0;

            FN = true;
            FZ = A == value;
        }
        private ushort Pop()
        {
            var p = RW(SP);
            SP += 2;
            return p;
        }
        private void Push(ushort value)
        {
            SP -= 2;
            WW(SP, value);
        }
        private void Reset(ushort pc)
        {
            SP -= 2;
            WW(SP, PC);
            PC = pc;
        }
        private byte RotateRightC(byte value, bool resetFz = false)
        {
            var lo = value.AND(1);
            var r = value.RS().OR(lo.LS(7));
            FC = lo == 1;
            FH = false;
            FN = false;

            // Some instructions unconditionally reset FZ even if the result is 0
            // This differs from the official docs, but several ref impl's confirm this behavior
            FZ = resetFz ? false : r == 0;
            return r;
        }
        private byte RotateLeftC(byte value, bool resetFz = false)
        {
            var hi = value.AND(128).RS(7);
            var r = value.LS().OR(hi);
            FC = hi == 1;
            FH = false;
            FN = false;

            // Some instructions unconditionally reset FZ even if the result is 0
            // This differs from the official docs, but several ref impl's confirm this behavior
            FZ = resetFz ? false : r == 0;
            return r;
        }
        private byte RotateRight(byte value)
        {
            var lo = value.AND(1);
            var r = value.RS().OR(FC.LS(7));
            FC = lo == 1;
            FH = false;
            FN = false;
            FZ = r == 0;
            return r;
        }
        private byte RotateLeft(byte value)
        {
            var hi = value.AND(128).RS(7);
            var r = value.LS().OR(FC);
            FC = hi == 1;
            FH = false;
            FN = false;
            FZ = r == 0;
            return r;
        }
        private byte ShiftRightMsb(byte value)
        {
            var lo = value.AND(1);
            var hi = value.AND(128);
            var r = value.RS().OR(hi); // Keep MSB
            FC = lo == 1;
            FH = false;
            FN = false;
            FZ = r == 0;
            return r;
        }
        private byte ShiftRight(byte value)
        {
            var lo = value.AND(1);
            var r = value.RS(); // Zero out MSB
            FC = lo == 1;
            FH = false;
            FN = false;
            FZ = r == 0;
            return r;
        }
        private byte ShiftLeft(byte value)
        {
            var hi = value.AND(128).RS(7);
            var r = value.LS();
            FC = hi == 1;
            FH = false;
            FN = false;
            FZ = r == 0;
            return r;
        }
        private byte Swap(byte value)
        {
            var hi = value.AND(_u4).LS(4);
            var lo = value.AND(240).RS(4);
            var r = hi.OR(lo);
            FC = false;
            FH = false;
            FN = false;
            FZ = r == 0;
            return r;
        }
        private void GetBit(byte value, byte bitMask)
        {
            FH = true;
            FN = false;
            FZ = value.AND(bitMask) == 0;
        }
    }
}