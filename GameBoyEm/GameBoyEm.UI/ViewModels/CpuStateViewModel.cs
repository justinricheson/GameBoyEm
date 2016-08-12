using GameBoyEm.Api;

namespace GameBoyEm.UI.ViewModels
{
    public class CpuStateViewModel : ViewModelBase
    {
        public string A { get; private set; }
        public string B { get; private set; }
        public string C { get; private set; }
        public string D { get; private set; }
        public string E { get; private set; }
        public string FC { get; private set; }
        public string FH { get; private set; }
        public string FN { get; private set; }
        public string FZ { get; private set; }
        public string H { get; private set; }
        public string L { get; private set; }
        public string SP { get; private set; }
        public string PC { get; private set; }
        public string IME { get; private set; }

        public CpuStateViewModel(ICpu cpu)
        {
            A = $"{cpu.A:X2}";
            B = $"{cpu.B:X2}";
            C = $"{cpu.C:X2}";
            D = $"{cpu.D:X2}";
            E = $"{cpu.E:X2}";
            FC = $"{cpu.FC}";
            FH = $"{cpu.FH}";
            FN = $"{cpu.FN}";
            FZ = $"{cpu.FZ}";
            H = $"{cpu.H:X2}";
            L = $"{cpu.L:X2}";
            SP = $"{cpu.SP:X4}";
            PC = $"{cpu.PC:X4}";
            IME = $"{cpu.IME}";
        }
    }
}
