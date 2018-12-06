using System;
using System.Windows.Input;

namespace DoxygenEditor.MVVM
{
    public class DelegateCommand<T> : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private readonly Action<T> _executeAction;
        private readonly Func<T, bool> _canExecuteAction;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }

        public DelegateCommand(Action<T> executeAction, Func<T, bool> canExecuteAction = null)
        {
            _executeAction = executeAction;
            _canExecuteAction = canExecuteAction;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecuteAction != null)
            {
                T value = (T)Convert.ChangeType(parameter, typeof(T));
                bool result = _canExecuteAction.Invoke(value);
                return (result);
            }
            return (true);
        }

        public void Execute(object parameter)
        {
            T value = (T)Convert.ChangeType(parameter, typeof(T));
            _executeAction?.Invoke(value);
        }
        public void Execute(T parameter)
        {
            _executeAction?.Invoke(parameter);
            RaiseCanExecuteChanged();
        }
    }
}
