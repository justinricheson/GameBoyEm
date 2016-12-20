using System;
using System.Windows.Input;

namespace GameBoyEm.UI.Commands
{
    public class ActionCommand : ICommand
    {
        private Action _a;
        public event EventHandler CanExecuteChanged;
        public ActionCommand(Action a) { _a = a; }
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter)
        {
            _a();
        }
    }
}
