using System;
using System.Windows.Input;

namespace TaskProgressAndCancel
{
    public class RelayCommand : ICommand
    {
        private Action<object> _execute;
        private Func<bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            if (_execute == null || (_canExecute != null && _canExecute() == false))
                return false;

            return true;
        }

        public void Execute(object parameter)
        {
            _execute.Invoke(parameter);
        }
    }
}
