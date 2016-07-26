using GameBoyEm.UI.ViewModels;
using System.Windows;

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
                    History.SelectedIndex = History.Items.Count - 1;
                    History.ScrollIntoView(History.SelectedItem);
                };
            }
        }
    }
}
