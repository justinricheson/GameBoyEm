using GameBoyEm.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GameBoyEm.UI
{
    public partial class DebuggerView : Window
    {
        public DebuggerView()
        {
            InitializeComponent();
        }

        private void Window_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = e.NewValue as DebuggerViewModel;
            if (vm != null)
            {
                vm.ScrollToBottom = () =>
                {
                    if (CpuHistory.Items.Count > 0)
                    {
                        var border = VisualTreeHelper.GetChild(CpuHistory, 0) as Decorator;
                        if (border != null)
                        {
                            var scroll = border.Child as ScrollViewer;
                            if (scroll != null)
                            {
                                scroll.ScrollToEnd();
                            }
                        }
                    }
                };
            }
        }
    }
}
