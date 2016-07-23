using GameBoyEm.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GameBoyEm.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = e.NewValue as MainViewModel;
            if (vm != null)
            {
                vm.UpdateScreen = UpdateScreen;
            }
        }

        private void UpdateScreen(WriteableBitmap bmp, IList<Color> frameBuffer)
        {
            bmp.Dispatcher.BeginInvoke((Action)(() =>
            {
                var bytesPerPixel = (bmp.Format.BitsPerPixel + 7) / 8;
                var stride = bmp.PixelWidth * bytesPerPixel;
                var arraySize = stride * bmp.PixelHeight;
                var array = new byte[arraySize];

                for (int i = 0; i < frameBuffer.Count; i++)
                {
                    var iPixel = i * 4;
                    var color = frameBuffer[i];
                    array[iPixel + 0] = color.A;
                    array[iPixel + 1] = color.R;
                    array[iPixel + 2] = color.G;
                    array[iPixel + 3] = color.B;
                }

                bmp.Lock();
                bmp.WritePixels(new Int32Rect(0, 0, 160, 144), array, stride, 0);
                bmp.Unlock();

                ScreenImage.Source = bmp;
            }));
        }
    }
}
