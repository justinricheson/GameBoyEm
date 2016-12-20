using GameBoyEm.Api;
using GameBoyEm.Cartridge;
using GameBoyEm.UI.Commands;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GameBoyEm.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private DebuggerViewModel _debuggerVm;
        private Console _console;
        private Console Console
        {
            get { return _console; }
            set
            {
                if (_console != null)
                {
                    _console.PowerOff();
                    _console.OnDrawScreen -= OnDrawScreen;
                }

                _console = value;
                _console.OnDrawScreen += OnDrawScreen;
                _debuggerVm = new DebuggerViewModel(_console);
            }
        }

        public string Title { get; private set; }
        public bool SaveStateEnabled
        {
            get { return _console.CartridgeLoaded && _console.TurnedOn; }
        }
        public bool PowerOnEnabled
        {
            get { return _console.CartridgeLoaded && !_console.TurnedOn; }
        }
        public bool PowerOffEnabled
        {
            get { return _console.TurnedOn; }
        }
        public bool ResetEnabled
        {
            get { return _console.TurnedOn; }
        }
        public bool DebuggerEnabled
        {
            get { return _console.TurnedOn; }
        }
        public bool PauseResumeEnabled
        {
            get { return _console.TurnedOn; }
        }
        public string PauseResumeLabel
        {
            get { return _console.Paused ? "Resume" : "Pause"; }
        }
        public bool IsPaused
        {
            get { return _console.Paused; }
            set
            {
                if (_console.Paused)
                {
                    _console.Resume();
                }
                else
                {
                    _console.Pause();
                }
                NotifyAll();
            }
        }
        public ICommand OpenCommand { get; private set; }
        public ICommand LoadStateCommand { get; private set; }
        public ICommand SaveStateCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }
        public ICommand PowerOnCommand { get; private set; }
        public ICommand PowerOffCommand { get; private set; }
        public ICommand ResetCommand { get; private set; }
        public ICommand DebuggerCommand { get; private set; }
        public Action<IList<Color>> UpdateScreen { get; set; }
        public Action<DebuggerViewModel> OpenDebuggerWindow { get; set; }

        public MainViewModel()
        {
            //Console = Console.Default();
            var mmu = new Mmu();
            Console = new Console(
                new Cpu(mmu), mmu,
                new Gpu(mmu,
                    // TODO make palettes configurable from UI
                    //Colors.White,
                    //Colors.LightGray,
                    //Colors.DarkGray,
                    //Colors.Black),
                    //new Color { R = 0xB8, G = 0xC2, B = 0x66 },
                    //new Color { R = 0x7B, G = 0x8A, B = 0x32 },
                    //new Color { R = 0x43, G = 0x59, B = 0x1D },
                    //new Color { R = 0x13, G = 0x2C, B = 0x13 }),
                    Colors.GhostWhite,
                    Colors.LightSlateGray,
                    Colors.DarkSlateBlue,
                    Colors.Black),
                new Timer(mmu),
                new Controller(mmu));

            Title = "GameboyEm";
            OpenCommand = new ActionCommand(Open);
            LoadStateCommand = new ActionCommand(LoadState);
            SaveStateCommand = new ActionCommand(SaveState);
            CloseCommand = new ActionCommand(Application.Current.Shutdown);
            PowerOnCommand = new ActionCommand(PowerOn);
            PowerOffCommand = new ActionCommand(PowerOff);
            ResetCommand = new ActionCommand(Reset);
            DebuggerCommand = new ActionCommand(Debugger);
        }

        public void OnKeyDown(Key key)
        {
            if (!_console.TurnedOn)
            {
                return;
            }

            switch (key)
            {
                case Key.X: _console.Controller.APressed = true; break;
                case Key.Z: _console.Controller.BPressed = true; break;
                case Key.Enter: _console.Controller.StartPressed = true; break;
                case Key.Space: _console.Controller.SelectPressed = true; break;
                case Key.Up: _console.Controller.UpPressed = true; break;
                case Key.Down: _console.Controller.DownPressed = true; break;
                case Key.Left: _console.Controller.LeftPressed = true; break;
                case Key.Right: _console.Controller.RightPressed = true; break;
                case Key.Escape: _console.Controller.FastPressed = true; break;
            }
        }

        public void OnKeyUp(Key key)
        {
            if (!_console.TurnedOn)
            {
                return;
            }

            switch (key)
            {
                case Key.X: _console.Controller.APressed = false; break;
                case Key.Z: _console.Controller.BPressed = false; break;
                case Key.Enter: _console.Controller.StartPressed = false; break;
                case Key.Space: _console.Controller.SelectPressed = false; break;
                case Key.Up: _console.Controller.UpPressed = false; break;
                case Key.Down: _console.Controller.DownPressed = false; break;
                case Key.Left: _console.Controller.LeftPressed = false; break;
                case Key.Right: _console.Controller.RightPressed = false; break;
                case Key.Escape: _console.Controller.FastPressed = false; break;
            }
        }

        private void Open()
        {
            var dlg = new OpenFileDialog
            {
                DefaultExt = ".gb",
                Filter = "Gameboy ROM Files (*.gb)|*.gb"
            };

            var result = dlg.ShowDialog();
            if (result == true)
            {
                try
                {
                    _console.PowerOff();
                    var cartridge = CartridgeBuilder.Build(
                        File.ReadAllBytes(dlg.FileName));
                    _console.LoadCartridge(cartridge);
                    Title = $"GameboyEm - {dlg.FileName}";
                    NotifyAll();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error", e.ToString(),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadState()
        {
            var dlg = new OpenFileDialog
            {
                DefaultExt = ".state",
                Filter = "Gameboy State Files (*.state)|*.state"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    using (var fs = new FileStream(dlg.FileName, FileMode.Open))
                    {
                        Console = (Console)new BinaryFormatter().Deserialize(fs);
                    }
                    NotifyAll();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error", e.ToString(),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveState()
        {
            var wasPaused = _console.Paused;
            _console.Pause();

            var dlg = new SaveFileDialog();
            dlg.FileName = "game";
            dlg.DefaultExt = ".state";
            dlg.Filter = "Gameboy State files (.state)|*.state";

            if (dlg.ShowDialog() == true)
            {
                using (var fs = new FileStream(dlg.FileName, FileMode.OpenOrCreate))
                {
                    new BinaryFormatter().Serialize(fs, _console);
                }
            }

            if (!wasPaused)
            {
                _console.Resume();
            }

            NotifyAll();
        }

        private void PowerOn()
        {
            try
            {
                _console.PowerOn();
            }
            catch (Exception e)
            {
                MessageBox.Show("Error", e.ToString(),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            NotifyAll();
        }

        private void PowerOff()
        {
            _console.PowerOff();
            NotifyAll();
        }

        private void Reset()
        {
            _console.Reset();
            NotifyAll();
        }

        private void Debugger()
        {
            _console.Pause();
            NotifyAll();

            OpenDebuggerWindow?.Invoke(_debuggerVm);

            _console.Resume();
            NotifyAll();
        }

        private void OnDrawScreen(object sender, DrawScreenEventArgs e)
        {
            UpdateScreen?.Invoke(e.FrameBuffer);
        }
    }
}
