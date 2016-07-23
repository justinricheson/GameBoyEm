﻿using GameBoyEm.Api;
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
        private WriteableBitmap _bmp;

        public ICommand OpenCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }
        public Action<WriteableBitmap, IList<Color>> UpdateScreen { get; set; }

        public MainViewModel()
        {
            OpenCommand = new AsyncCommand(Open);
            CloseCommand = new ActionCommand(Application.Current.Shutdown);
            _bmp = new WriteableBitmap(160, 144, 90, 90, PixelFormats.Bgr32, null);
        }

        private async Task Open()
        {
            var dlg = new OpenFileDialog
            {
                DefaultExt = ".gb",
                Filter = "Gameboy ROM Files (*.gb)|*.gb"
            };

            var result = dlg.ShowDialog();
            if (result == true)
            {
                await LoadCartridge(dlg.FileName);
            }
        }

        private async Task LoadCartridge(string path)
        {
            var cartridge = CartridgeBuilder.Build(
                File.ReadAllBytes(path));

            var console = Console.Default();
            console.LoadCartridge(cartridge);
            console.OnDrawScreen += OnDrawScreen;
            await console.PowerOn();
        }

        private void OnDrawScreen(object sender, DrawScreenEventArgs e)
        {
            UpdateScreen?.Invoke(_bmp, e.FrameBuffer);
        }
    }
}
