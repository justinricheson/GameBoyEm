using GameBoyEm.Api;
using GameBoyEm.Cartridge;
using GameBoyEm.UI.Commands;
using Microsoft.Win32;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GameBoyEm.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ICommand OpenCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }

        public MainViewModel()
        {
            OpenCommand = new AsyncCommand(Open);
            CloseCommand = new ActionCommand(Application.Current.Shutdown);
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

        }
    }
}
