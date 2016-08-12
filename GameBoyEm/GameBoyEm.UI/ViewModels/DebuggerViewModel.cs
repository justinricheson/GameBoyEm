using GameBoyEm.Api;
using GameBoyEm.UI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GameBoyEm.UI.ViewModels
{
    public class DebuggerViewModel : ViewModelBase
    {
        private Console _console;
        private ICpu _cpu;
        private uint _steps;
        private bool _enableDebugger;
        private bool _enableCpuHistory;
        private bool _enableCpuHistoryEnabled;
        private bool _progressVisible;
        private int _progressMax;
        private int _progressValue;
        private ObservableCollection<CpuStateViewModel> _cpuHistory;

        public CpuStateViewModel Current { get; private set; }
        public ObservableCollection<CpuStateViewModel> CpuHistory
        {
            get { return _cpuHistory; }
            set
            {
                if (_cpuHistory != value)
                {
                    _cpuHistory = value;
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
                    EnableCpuHistoryEnabled = _steps <= 1000;
                    if (!EnableCpuHistoryEnabled)
                    {
                        EnableCpuHistory = false;
                    }

                    Notify();
                }
            }
        }
        public bool EnableCpuHistory
        {
            get { return _enableCpuHistory; }
            set
            {
                if (_enableCpuHistory != value)
                {
                    _enableCpuHistory = value;
                    Notify();
                }
            }
        }
        public bool EnableCpuHistoryEnabled
        {
            get { return _enableCpuHistoryEnabled; }
            set
            {
                if (_enableCpuHistoryEnabled != value)
                {
                    _enableCpuHistoryEnabled = value;
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
        public ICommand ClearCommand { get; set; }
        public ICommand StepCommand { get; set; }
        public Action ScrollToBottom { get; set; }

        public DebuggerViewModel(Console console)
        {
            _enableDebugger = true;
            _console = console;
            _cpu = _console.Cpu;
            Steps = 1;
            CpuHistory = new ObservableCollection<CpuStateViewModel>();
            ClearCommand = new ActionCommand(Clear);
            StepCommand = new ActionCommand(Step);
            Refresh();
        }

        public void Refresh()
        {
            Current = new CpuStateViewModel(_cpu);
            CpuHistory.Clear();
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
                EndStep(StartStep(p => p()));
            }
            else
            {
                var t = new Thread(() =>
                {
                    var history = StartStep(App.Current.Dispatcher.Invoke);
                    App.Current.Dispatcher.Invoke(() => { EndStep(history); });
                });
                t.Start();
            }
        }

        private IEnumerable<CpuStateViewModel> StartStep(Action<Action> progress)
        {
            var history = new List<CpuStateViewModel>();

            _console.Step((int)Steps, i =>
            {
                if (EnableCpuHistory)
                {
                    history.Add(new CpuStateViewModel(_cpu));
                }
                if (i % 100000 == 0)
                {
                    progress(() => ProgressValue = i);
                }
            });

            return history;
        }

        private void EndStep(IEnumerable<CpuStateViewModel> history)
        {
            if (EnableCpuHistory)
            {
                foreach (var state in history)
                {
                    CpuHistory.Add(state);
                };
            }

            Current = new CpuStateViewModel(_cpu);
            ScrollToBottom?.Invoke();
            NotifyAll();

            ProgressVisible = false;
            EnableDebugger = true;
        }

        private void Clear()
        {
            CpuHistory.Clear();
        }
    }
}
