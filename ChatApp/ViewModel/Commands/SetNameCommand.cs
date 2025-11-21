using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatApp.ViewModel.Commands
{
    internal class SetNameCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private MainWindowViewModel parent = null;

        public SetNameCommand(MainWindowViewModel parent)
        {
            this.parent = parent;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            Debug.WriteLine("Set port   ...");
            parent.setNameUser();
        }
    }
}
