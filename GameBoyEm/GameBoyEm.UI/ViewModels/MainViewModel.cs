using GameBoyEm.Cartridge;
using System.IO;

namespace GameBoyEm.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public void LoadCartridge(string path)
        {
            var cartridge = CartridgeBuilder.Build(
                File.ReadAllBytes(path));

            var console = Console.Default();
            console.LoadCartridge(cartridge);
            console.PowerOn();
        }
    }
}
