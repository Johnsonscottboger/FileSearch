using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FileSerach.Command
{
    internal class RelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<Object, Task> _executeWithParam;
        private readonly Func<bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (this._canExecute != null)
                    CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (this._canExecute != null)
                    CommandManager.RequerySuggested -= value;
            }
        }

        public RelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            this._execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this._canExecute = canExecute;
        }

        public RelayCommand(Func<Object, Task> execute, Func<bool> canExecute = null)
        {
            this._executeWithParam = execute ?? throw new ArgumentNullException(nameof(execute));
            this._canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return this._canExecute == null ? true : this._canExecute();
        }

        public async void Execute(object parameter)
        {
            if (parameter == null)
                await this._execute();
            else
                await this._executeWithParam(parameter);
        }
    }
}
