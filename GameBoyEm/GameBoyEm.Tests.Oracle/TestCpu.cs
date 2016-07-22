using GameBoyEm.Interfaces;

namespace GameBoyEm.Tests.Oracle
{
    /// <summary>
    /// Allows initial state to be set for testing
    /// </summary>
    public class TestCpu : Cpu
    {
        public TestCpu(IMmu mmu,
            byte a, byte b,
            byte c, byte d,
            byte e, byte h, byte l,
            ushort sp, ushort pc,
            bool fz, bool fn,
            bool fh, bool fc,
            bool ime) : base(mmu)
        {
            A = a;
            B = b;
            C = c;
            D = d;
            E = e;
            H = h;
            L = l;
            SP = sp;
            PC = pc;
            FZ = fz;
            FN = fn;
            FH = fh;
            FC = fc;
            IME = ime;
        }
    }
}
