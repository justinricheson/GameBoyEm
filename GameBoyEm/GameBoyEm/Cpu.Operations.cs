using static GameBoyEm.CpuCycles;

namespace GameBoyEm
{
    public partial class Cpu
    {
        // Misc
        private void NA() { } // Stub for invalid opcodes
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
        private void LDNA() { WB(RW(PC), A); PC += 2; }
        private void LDHLMN() { WB(HL, RB(PC++)); }
        private void LDIHLA() { WB(HL++, A); }
        private void LDDHLA() { WB(HL--, A); }
        private void LDIAHL() { A = RB(HL++); }
        private void LDDAHL() { A = RB(HL--); }
        private void LDIOAN() { WB((ushort)(_io + PC++), A); }
        private void LDIOAC() { A = RB((ushort)(_io + C)); }

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
        private void ADDN() { Add(RB(PC++)); }
        private void ADCA() { AddC(A); }
        private void ADCB() { AddC(B); }
        private void ADCC() { AddC(C); }
        private void ADCD() { AddC(D); }
        private void ADCE() { AddC(E); }
        private void ADCH() { AddC(H); }
        private void ADCL() { AddC(L); }
        private void ADCHL() { AddC(RB(HL)); }
        private void ADCN() { AddC(RB(PC++)); }
        private void SUBA() { Sub(A); }
        private void SUBB() { Sub(B); }
        private void SUBC() { Sub(C); }
        private void SUBD() { Sub(D); }
        private void SUBE() { Sub(E); }
        private void SUBH() { Sub(H); }
        private void SUBL() { Sub(L); }
        private void SUBHL() { Sub(RB(HL)); }
        private void SUBN() { Sub(RB(PC++)); }
        private void SBCA() { SubC(A); }
        private void SBCB() { SubC(B); }
        private void SBCC() { SubC(C); }
        private void SBCD() { SubC(D); }
        private void SBCE() { SubC(E); }
        private void SBCH() { SubC(H); }
        private void SBCL() { SubC(L); }
        private void SBCHL() { SubC(RB(HL)); }
        private void SBCN() { SubC(RB(PC++)); }
        private void ANDA() { And(A); }
        private void ANDB() { And(B); }
        private void ANDC() { And(C); }
        private void ANDD() { And(D); }
        private void ANDE() { And(E); }
        private void ANDH() { And(H); }
        private void ANDL() { And(L); }
        private void ANDHL() { And(RB(HL)); }
        private void ANDN() { And(RB(PC++)); }
        private void XORA() { Xor(A); }
        private void XORB() { Xor(B); }
        private void XORC() { Xor(C); }
        private void XORD() { Xor(D); }
        private void XORE() { Xor(E); }
        private void XORH() { Xor(H); }
        private void XORL() { Xor(L); }
        private void XORHL() { Xor(RB(HL)); }
        private void XORN() { Xor(RB(PC++)); }
        private void ORA() { Or(A); }
        private void ORB() { Or(B); }
        private void ORC() { Or(C); }
        private void ORD() { Or(D); }
        private void ORE() { Or(E); }
        private void ORH() { Or(H); }
        private void ORL() { Or(L); }
        private void ORHL() { Or(RB(HL)); }
        private void CPA() { Compare(A); }
        private void CPB() { Compare(B); }
        private void CPC() { Compare(C); }
        private void CPD() { Compare(D); }
        private void CPE() { Compare(E); }
        private void CPH() { Compare(H); }
        private void CPL() { Compare(L); }
        private void CPHL() { Compare(RB(HL)); }
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
        private void ADDHLBC() { HL = Add16(BC, HL); }
        private void ADDHLDE() { HL = Add16(DE, HL); }
        private void ADDHLHL() { HL = Add16(HL, HL); }
        private void ADDHLSP() { HL = Add16(SP, HL); }
        private void ADDSPN() { SP = Add16(RB(PC++), SP); }

        // Rotates and Shifts
        private void RRA() { var lo = A.AND(1); A = A.RS().OR(FC.LS(7)); F = 0; FC = lo == 1; FZ = A == 0; }
        private void RLA() { var hi = A.AND(128); A = A.LS().OR(FC); F = 0; FC = hi == 128; FZ = A == 0; }
        private void RRCA() { var lo = A.AND(1).LS(7); A = A.RS().OR(lo); F = 0; FC = lo == 128; FZ = A == 0; }
        private void RLCA() { var hi = A.AND(128).RS(7); A = A.LS().OR(hi); F = 0; FC = hi == 1; FZ = A == 0; }
        private void SRLB() { var lo = B.AND(1); B = B.RS(); F = 0; FC = lo == 1; FZ = B == 0; }

        // Jumps, Calls, Returns, and Resets
        private void JR() { JumpRel(); }
        private void JRZ() { JumpRel(FZ); }
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

        // Push and Pop
        private void POPBC() { BC = Pop(); }
        private void POPDE() { DE = Pop(); }
        private void POPHL() { HL = Pop(); }
        private void PUSHBC() { Push(BC); }
        private void PUSHDE() { Push(DE); }
        private void PUSHHL() { Push(HL); }

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

        private ushort Add16(ushort x, ushort y, bool resetFZ = false)
        {
            int add = x + y;
            FC = add > _u16;
            FH = (x & _u12) + (y & _u12) > _u12;
            FN = false;
            if (resetFZ) FZ = false;
            return (ushort)add;
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
    }
}
