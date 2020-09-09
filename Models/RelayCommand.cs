using System;
using System.Windows.Input;

namespace FFXIVOpcodeWizard.Models
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> executeMethod;
        private readonly Predicate<object> canExecuteMethod;

        public RelayCommand(Action<object> executeMethod, Predicate<object> canExecuteMethod = null)
        {
            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecuteMethod?.Invoke(parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            this.executeMethod?.Invoke(parameter);
        }

        public event EventHandler CanExecuteChanged;
    }
}