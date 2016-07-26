using GameBoyEm.Api;
using GameBoyEm.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace GameBoyEm.UI.ViewModels
{
    public class DebuggerViewModel : ViewModelBase
    {
        private Console _console;
        private ICpu _cpu;

        public string A { get { return $"{_cpu.A:X2}"; } }
        public string B { get { return $"{_cpu.B:X2}"; } }
        public string C { get { return $"{_cpu.C:X2}"; } }
        public string D { get { return $"{_cpu.D:X2}"; } }
        public string FC { get { return $"{_cpu.FC}"; } }
        public string FH { get { return $"{_cpu.FH}"; } }
        public string FN { get { return $"{_cpu.FN}"; } }
        public string FZ { get { return $"{_cpu.FZ}"; } }
        public string H { get { return $"{_cpu.H:X2}"; } }
        public string L { get { return $"{_cpu.L:X2}"; } }
        public string SP { get { return $"{_cpu.SP:X4}"; } }
        public string PC { get { return $"{_cpu.PC:X4}"; } }
        public string IME { get { return $"{_cpu.IME}"; } }
        
        public ObservableCollection<string> History { get; set; }
        public ICommand ClearCommand { get; set; }
        public ICommand StepCommand { get; set; }
        public Action ScrollToBottom { get; set; }

        public DebuggerViewModel(Console console)
        {
            _console = console;
            _cpu = _console.Cpu;
            History = new ObservableCollection<string>();
            ClearCommand = new ActionCommand(Clear);
            StepCommand = new ActionCommand(Step);
        }

        private void Step()
        {
            History.Add($"{A}|{B}|{C}|{D}|{FC}|{FH}|{FN}|{FZ}|{H}|{L}|{SP}|{PC}|{IME}");
            ScrollToBottom?.Invoke();
            _console.Step();
            Notify(null);
        }

        private void Clear()
        {
            History.Clear();
        }
    }
}
