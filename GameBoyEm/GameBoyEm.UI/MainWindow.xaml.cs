using GameBoyEm.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GameBoyEm.UI
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private WriteableBitmap _bmp;
        private byte[] _array;
        private int _stride;

        public MainWindow()
        {
            _bmp = new WriteableBitmap(160, 144, 90, 90, PixelFormats.Bgr32, null);
            var bytesPerPixel = (_bmp.Format.BitsPerPixel + 7) / 8;
            _stride = _bmp.PixelWidth * bytesPerPixel;
            _array = new byte[_stride * _bmp.PixelHeight];

            InitializeComponent();
        }

        private void Window_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = e.NewValue as MainViewModel;
            if (vm != null)
            {
                _viewModel = vm;
                _viewModel.UpdateScreen = UpdateScreen;
                _viewModel.OpenDebuggerWindow = OpenDebuggerWindow;
            }
        }

        private void OpenDebuggerWindow(Console console)
        {
            var window = new DebuggerView
            {
                DataContext = new DebuggerViewModel(console),
                Owner = this
            };

            window.ShowDialog();
        }

        private void UpdateScreen(IList<Color> frameBuffer)
        {
            _bmp.Dispatcher.BeginInvoke((Action)(() =>
            {
                for (int i = 0; i < frameBuffer.Count; i++)
                {
                    var iPixel = i * 4;
                    var color = frameBuffer[i];
                    _array[iPixel + 0] = color.B;
                    _array[iPixel + 1] = color.G;
                    _array[iPixel + 2] = color.R;
                    _array[iPixel + 3] = color.A;
                }

                _bmp.Lock();
                _bmp.WritePixels(new Int32Rect(0, 0, 160, 144), _array, _stride, 0);
                _bmp.Unlock();

                ScreenImage.Source = _bmp;
            }));
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _viewModel.OnKeyDown(e.Key);
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            _viewModel.OnKeyUp(e.Key);
        }
    }
}
