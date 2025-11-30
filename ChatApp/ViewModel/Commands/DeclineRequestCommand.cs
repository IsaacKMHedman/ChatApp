using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatApp.ViewModel.Commands
{
    internal class RejectRequestCommand : ICommand
    {

        public event EventHandler CanExecuteChanged;
        private AcceptRequestWindowViewModel parent = null;

        public RejectRequestCommand(AcceptRequestWindowViewModel parent)
        {
            this.parent = parent;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            parent.rejectRequestFunction();
        }
    }
}
