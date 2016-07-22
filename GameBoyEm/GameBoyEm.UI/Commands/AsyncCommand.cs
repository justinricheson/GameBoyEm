using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GameBoyEm.UI.Commands
{
    public class AsyncCommand : ICommand
    {
        private Func<Task> _f;
        public event EventHandler CanExecuteChanged;
        public AsyncCommand(Func<Task> f) { _f = f; }
        public bool CanExecute(object parameter) => true;
        public async void Execute(object parameter) => await _f();
    }
}
