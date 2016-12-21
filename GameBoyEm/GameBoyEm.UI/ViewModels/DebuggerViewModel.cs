using GameBoyEm.Api;
using GameBoyEm.UI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace GameBoyEm.UI.ViewModels
{
    public class DebuggerViewModel : ViewModelBase
    {
        private Console _console;
        private uint _steps;
        private bool _enableDebugger;
        private bool _progressVisible;
        private int _progressMax;
        private int _progressValue;
        private ushort? _breakpoint;
        private MmuByteProvider _mmuByteProvider;

        public Action InvalidateHexBox { get; set; }
        public MmuByteProvider ByteProvider { get { return _mmuByteProvider; } }
        public CpuStateViewModel Current { get; private set; }
        public ObservableCollection<ushort> Breakpoints { get; private set; }
        public int SelectedBreakpointIndex { get; set; }
        public string Breakpoint
        {
            get { return $"{_breakpoint:X4}"; }
            set
            {
                var parsed = Convert.ToUInt16(value, 16);
                if (parsed != _breakpoint)
                {
                    _breakpoint = parsed;
                    Notify();
                }
            }
        }
        public uint Steps
        {
            get { return _steps; }
            set
            {
                if (_steps != value)
                {
                    _steps = value;
                    Notify();
                }
            }
        }
        public bool EnableDebugger
        {
            get { return _enableDebugger; }
            set
            {
                if (_enableDebugger != value)
                {
                    _enableDebugger = value;
                    Notify();
                }
            }
        }
        public bool ProgressVisible
        {
            get { return _progressVisible; }
            set
            {
                if (_progressVisible != value)
                {
                    _progressVisible = value;
                    Notify();
                }
            }
        }
        public int ProgressMax
        {
            get { return _progressMax; }
            set
            {
                if (_progressMax != value)
                {
                    _progressMax = value;
                    Notify();
                }
            }
        }
        public int ProgressValue
        {
            get { return _progressValue; }
            set
            {
                if (_progressValue != value)
                {
                    _progressValue = value;
                    Notify();
                }
            }
        }
        public ICommand StepCommand { get; set; }
        public ICommand AddBreakpointCommand { get; set; }
        public ICommand RemoveBreakpointCommand { get; set; }

        public DebuggerViewModel(Console console)
        {
            _enableDebugger = true;
            _console = console;
            _console.OnBreakpoint += OnBreakpoint;
            _mmuByteProvider = new MmuByteProvider(_console.Mmu);

            Steps = 1;
            Breakpoints = new ObservableCollection<ushort>();
            StepCommand = new ActionCommand(Step);
            AddBreakpointCommand = new ActionCommand(AddBreakpoint);
            RemoveBreakpointCommand = new ActionCommand(RemoveBreakpoint);
            Refresh();
        }

        public void Refresh()
        {
            Current = new CpuStateViewModel(_console.Cpu);
        }

        private void Step()
        {
            EnableDebugger = false;
            ProgressVisible = true;
            ProgressValue = 0;
            ProgressMax = (int)Steps;

            if (Steps <= 1000)
            {
                // Avoid creating the thread for small step counts
                // which slows down the ui for fast clicks
                StartStep(p => p());
                EndStep();
            }
            else
            {
                var t = new Thread(() =>
                {
                    StartStep(App.Current.Dispatcher.Invoke);
                    App.Current.Dispatcher.Invoke(() => { EndStep(); });
                });
                t.Start();
            }
        }

        private void StartStep(Action<Action> progress)
        {
            _console.StepMany((int)Steps, i =>
            {
                if (i % 100000 == 0)
                {
                    progress(() => ProgressValue = i);
                }
            });
        }

        private void EndStep()
        {
            _mmuByteProvider.InvokeChanged();
            InvalidateHexBox?.Invoke();
            Current = new CpuStateViewModel(_console.Cpu);
            NotifyAll();

            ProgressVisible = false;
            EnableDebugger = true;
        }

        private void AddBreakpoint()
        {
            if (_breakpoint != null && !Breakpoints.Contains(_breakpoint.Value))
            {
                Breakpoints.Add(_breakpoint.Value);
                _console.SetBreakpoint(_breakpoint.Value);
            }
        }

        private void RemoveBreakpoint()
        {
            if (_breakpoint != null && SelectedBreakpointIndex >= 0)
            {
                Breakpoints.RemoveAt(SelectedBreakpointIndex);
                _console.UnsetBreakpoint(_breakpoint.Value);
            }
        }

        private void OnBreakpoint(object sender, BreakpointEventArgs e)
        {
            // TODO if debugger not open, offer to open
            // TODO option to pause execution
            var result = MessageBox.Show($"Remove breakpoint 0x{e.PC:X4}?", "Breakpoint hit",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Breakpoints.Remove(e.PC);
                    _console.UnsetBreakpoint(e.PC);
                });
            }
        }
    }
}
