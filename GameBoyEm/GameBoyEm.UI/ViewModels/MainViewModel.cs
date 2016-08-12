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

        public string Title { get; private set; }
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
        public ICommand CloseCommand { get; private set; }
        public ICommand PowerOnCommand { get; private set; }
        public ICommand PowerOffCommand { get; private set; }
        public ICommand ResetCommand { get; private set; }
        public ICommand DebuggerCommand { get; private set; }
        public Action<IList<Color>> UpdateScreen { get; set; }
        public Action<Console> OpenDebuggerWindow { get; set; }

        public MainViewModel()
        {
            _console = Console.Default();
            _console.OnDrawScreen += OnDrawScreen;

            Title = "GameboyEm";
            OpenCommand = new ActionCommand(Open);
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
                var cartridge = CartridgeBuilder.Build(
                    File.ReadAllBytes(dlg.FileName));
                _console.LoadCartridge(cartridge);
                Title = $"GameboyEm - {dlg.FileName}";
                NotifyAll();
            }
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

            OpenDebuggerWindow?.Invoke(_console);

            _console.Resume();
            NotifyAll();
        }

        private void OnDrawScreen(object sender, DrawScreenEventArgs e)
        {
            UpdateScreen?.Invoke(e.FrameBuffer);
        }
    }
}
