using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatApp.ViewModel.Commands
{
    internal class SetFriendPortCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private MainWindowViewModel parent = null;

        public SetFriendPortCommand(MainWindowViewModel parent)
        {
            this.parent = parent;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            Debug.WriteLine("Setting Friend Port");
            parent.setPortFriend();
        }
    }
}
