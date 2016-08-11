using GameBoyEm.Api;
using GameBoyEm.Cartridge;
using GameBoyEm.UI.Commands;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GameBoyEm.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private Console _console;
        private string _cartridgePath;
        private string _title;
        private bool _powerOnEnabled;
        private bool _powerOffEnabled;
        private bool _debuggerEnabled;

        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    Notify();
                }
            }
        }
        public bool PowerOnEnabled
        {
            get { return _powerOnEnabled; }
            set
            {
                if (_powerOnEnabled != value)
                {
                    _powerOnEnabled = value;
                    Notify();
                }
            }
        }
        public bool PowerOffEnabled
        {
            get { return _powerOffEnabled; }
            set
            {
                if (_powerOffEnabled != value)
                {
                    _powerOffEnabled = value;
                    Notify();
                }
            }
        }
        public bool DebuggerEnabled
        {
            get { return _debuggerEnabled; }
            set
            {
                if (_debuggerEnabled != value)
                {
                    _debuggerEnabled = value;
                    Notify();
                }
            }
        }
        public ICommand OpenCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }
        public ICommand PowerOnCommand { get; private set; }
        public ICommand PowerOffCommand { get; private set; }
        public ICommand DebuggerCommand { get; set; }
        public Action<IList<Color>> UpdateScreen { get; set; }
        public Action<Console> OpenDebuggerWindow { get; set; }

        public MainViewModel()
        {
            _console = Console.Default();

            Title = "GameboyEm";
            OpenCommand = new ActionCommand(Open);
            CloseCommand = new ActionCommand(Application.Current.Shutdown);
            PowerOnCommand = new AsyncCommand(PowerOn);
            PowerOffCommand = new ActionCommand(PowerOff);
            DebuggerCommand = new AsyncCommand(Debugger);
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
                _cartridgePath = dlg.FileName;
                PowerOnEnabled = true;
                PowerOffEnabled = false;
                DebuggerEnabled = false;
                Title = $"GameboyEm - {dlg.FileName}";
            }
        }

        private async Task PowerOn()
        {
            try
            {
                PowerOnEnabled = false;
                PowerOffEnabled = true;
                DebuggerEnabled = true;

                var cartridge = CartridgeBuilder.Build(
                    File.ReadAllBytes(_cartridgePath));

                _console.LoadCartridge(cartridge);
                _console.OnDrawScreen += OnDrawScreen;
                await _console.PowerOn();
            }
            catch (Exception e)
            {
                MessageBox.Show("Error", e.ToString(),
                    MessageBoxButton.OK, MessageBoxImage.Error);

                PowerOnEnabled = true;
                PowerOffEnabled = false;
                DebuggerEnabled = false;
            }
        }

        private void PowerOff()
        {
            _console.PowerOff();
            PowerOnEnabled = true;
            PowerOffEnabled = false;
            DebuggerEnabled = false;
        }

        private async Task Debugger()
        {
            _console.Pause();

            OpenDebuggerWindow?.Invoke(_console);

            await _console.Resume();
        }

        private void OnDrawScreen(object sender, DrawScreenEventArgs e)
        {
            UpdateScreen?.Invoke(e.FrameBuffer);
        }
    }
}
