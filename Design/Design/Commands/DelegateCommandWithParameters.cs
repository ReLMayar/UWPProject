using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Design.Commands
{
    public class DelegateCommandWithParameters<T> : ICommand
    {
        private readonly Action<T> executeAction;
        Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged;

        public DelegateCommandWithParameters(Action<T> executeAction)
        : this(executeAction, null)
        {
            //var a = ((Page)(((Func<object, bool>)(executeAction.Target)).Target)).Name;
            //((ViewModel.VMBranchSelection)(executeAction.Target)).;

        }

        public DelegateCommandWithParameters(Action<T> executeAction, Func<object, bool> canExecute)
        {
            this.executeAction = executeAction;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null ? true : canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            executeAction((T)parameter);
        }
        public void RaiseCanExecuteChanged()
        {
            EventHandler handler = this.CanExecuteChanged;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }
    }
}
