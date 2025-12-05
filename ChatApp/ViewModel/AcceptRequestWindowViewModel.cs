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
    internal class AcceptRequestWindowViewModel : INotifyPropertyChanged
    {

        private NetworkManager _networkManager;
        private ICommand acceptRequest;
        private ICommand rejectRequest;
        public event Action? CloseAction;
        public event Action? AcceptAction;
        public event Action? RejectAction;

        public string FriendName => _networkManager.FriendName;
        public string FriendPort => _networkManager.FriendPort;


        public AcceptRequestWindowViewModel(NetworkManager networkManager)
        {
            _networkManager = networkManager;
            _networkManager.PropertyChanged += NetworkManagerOnPropertyChanged;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        //Det här behövs ses över.
        private void NetworkManagerOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NetworkManager.Message))
            {
                OnPropertyChanged(nameof(NetworkManager.Message));
            }
        }
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        
    }
}
