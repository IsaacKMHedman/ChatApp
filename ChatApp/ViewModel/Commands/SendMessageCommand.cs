using System;
using System.Diagnostics;
using System.Windows.Input;

namespace ChatApp.ViewModel.Commands
{
    internal class SendMessageCommand : ICommand
    {

        public event EventHandler CanExecuteChanged;
        private MainWindowViewModel parent = null;

        public SendMessageCommand(MainWindowViewModel parent)
        {
            this.parent = parent;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            parent.sendMessageAsync();
        }
    }
}