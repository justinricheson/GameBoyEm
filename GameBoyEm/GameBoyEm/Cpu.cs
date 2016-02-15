namespace GameBoyEm
{
    /// <summary>
    /// Emulates a modified Zilog Z80, 8-bit processor used in the original GameBoy
    /// </summary>
    public class Cpu : ICpu
    {
        // Registers
        public byte A { get; set; }
        public byte B { get; set; }
        public byte C { get; set; }
        public byte D { get; set; }
        public byte E { get; set; }
        public byte H { get; set; }
        public byte L { get; set; }
        public byte F { get; set; } // Flags
        public byte SP { get; set; } // Stack pointer
        public ushort PC { get; set; } // Program counter

        // Timers (last instruction)
        public ulong M { get; set; }
        public ulong T { get; set; }

        // Cumulative instruction timers
        public ulong TotalM { get; set; }
        public ulong TotalT { get; set; }

        private IMmu _mmu;

        public Cpu(IMmu mmu)
        {
            _mmu = mmu;
        }

        public void Run()
        {
            Init();
            while (true)
            {
                var op = _mmu.ReadByte(PC++); // Fetch
                var decoded = OpDecoder.Decode(op); // Decode
                decoded(this); // Execute

                TotalM += M;
                TotalT += T;
            }
        }

        private void Init()
        {
            A = B = C = D = E = H = L = F = SP = 0;
            M = T = TotalM = TotalT = 0;
            PC = 0;
        }
    }
}
