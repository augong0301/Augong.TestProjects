using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Augong.UI
{
    public class RelayCommand : ICommand
    {
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            mExecute    = execute;
            mCanExecute = canExecute;
        }

        private Action<object>    mExecute;
        private Predicate<object> mCanExecute;

        public event EventHandler CanExecuteChanged
        {
            add    => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) => mCanExecute == null ? true : mCanExecute(parameter);
 
        public void Execute(object parameter) => mExecute(parameter);
         
    }
}
