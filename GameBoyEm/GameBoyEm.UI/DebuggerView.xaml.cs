using Be.Windows.Forms;
using GameBoyEm.UI.ViewModels;
using System.Drawing;
using System.Windows;
using System.Windows.Forms.Integration;

namespace GameBoyEm.UI
{
    public partial class DebuggerView : Window
    {
        private HexBox _hexBox;

        public DebuggerView(MmuByteProvider mmuByteProvider)
        {
            _hexBox = new HexBox
            {
                AllowDrop = false,
                LineInfoVisible = true,
                ColumnInfoVisible = true,
                UseFixedBytesPerLine = true,
                VScrollBarVisible = true,
                BytesPerLine = 16,
                HexCasing = HexCasing.Lower,
                ByteCharConverter = new DefaultByteCharConverter(),
                Font = new Font(
                    System.Drawing.SystemFonts.MessageBoxFont.FontFamily,
                    System.Drawing.SystemFonts.MessageBoxFont.Size,
                    System.Drawing.SystemFonts.MessageBoxFont.Style),
                ByteProvider = mmuByteProvider
            };

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            hexboxHost.Children.Add(new WindowsFormsHost
            {
                Child = _hexBox
            });
        }

        private void Window_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = e.NewValue as DebuggerViewModel;
            if (vm != null)
            {
                vm.InvalidateHexBox = () => _hexBox.Invalidate();
            }
        }
    }
}
