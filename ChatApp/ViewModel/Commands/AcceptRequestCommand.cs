using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatApp.ViewModel.Commands
{
    internal class AcceptRequestCommand : ICommand
    {

        public event EventHandler CanExecuteChanged;
        private AcceptRequestWindowViewModel parent = null;

        public AcceptRequestCommand(AcceptRequestWindowViewModel parent)
        {
            this.parent = parent;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            parent.acceptRequestFunction();
        }
    }
}
