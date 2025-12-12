using ChatApp.Model;
using ChatApp.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ChatApp.ViewModel
{
    internal class AcceptRequestWindowViewModel
    {

        public event Action? CloseAction;
        public event Action? AcceptAction;
        public event Action? RejectAction;

        private NetworkManager _networkManager;

        private ICommand acceptRequest;
        private ICommand rejectRequest;

        public string FriendName => _networkManager.FriendName;
        public string FriendPort => _networkManager.FriendPort;

        public AcceptRequestWindowViewModel(NetworkManager networkManager)
        {
            _networkManager = networkManager;
        }

        //Klickar användaren på reject eller accept kör den lite olika saker. 
        //Dessa actions ligger i mainwindowviewmodel där vi skapar den här accept-rutan
        //Networkmanager.RejectConnection/AcceptConnection körs och sätter en TaskCompletionSource till respektive värde.
        //Det är då networkmanager kan gå vidare
        public void rejectRequestFunction()
        {
            RejectAction?.Invoke();
            _networkManager.RejectConnection();
            CloseAction?.Invoke();
        }

        public void acceptRequestFunction()
        {
            _networkManager.AcceptConnection();
            CloseAction?.Invoke();
            AcceptAction?.Invoke();
        }
        public ICommand AcceptRequest
        {
            get
            {
                if (acceptRequest == null)
                {
                    acceptRequest = new AcceptRequestCommand(this);
                }
                return acceptRequest;

            }
            set
            {
                acceptRequest = value;
            }
        }
        public ICommand RejectRequest
        {
            get
            {
                if (rejectRequest == null)
                {
                    rejectRequest = new RejectRequestCommand(this);
                }
                return rejectRequest;

            }
            set
            {
                rejectRequest = value;
            }
        }
        
    }
}
