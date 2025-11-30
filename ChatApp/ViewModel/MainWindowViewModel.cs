using ChatApp.Model;
using ChatApp.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ChatApp.ViewModel
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {

        private NetworkManager _networkManager { get; set; }
        private ICommand sendMessageButtonCommand;
        private ICommand setPort;
        private ICommand setNameUserCommand;
        private ICommand setFriendPortCommand;
        private ICommand startServerCommand;

        //Det här är tänkt så att man ska kunna ändra texten i chatrutan. 
        public String ChatText => _networkManager.Message;

        private String portVm;
        private String nameUser;
        private String friendPort;
        public string PortVm
        {
            get => portVm;
            set
            {
                portVm = value; OnPropertyChanged(nameof(PortVm));
            }
        }
        private String sendMessageTextBox;
        public string SendMessageTextBox
        {
            get => sendMessageTextBox;
            set
            {
                sendMessageTextBox = value; OnPropertyChanged(nameof(SendMessageTextBox));
            }
        }

        public string NameUser
        {
            get => nameUser;
            set
            {
                nameUser = value; OnPropertyChanged(nameof(NameUser));
            }
        }

        public string FriendPort
        {
            get => friendPort;
            set
            {
                friendPort = value; OnPropertyChanged(nameof(FriendPort));
            }
        }

        public MainWindowViewModel(NetworkManager networkManager)
        {

            _networkManager = networkManager;
            _networkManager.PropertyChanged += NetworkManagerOnPropertyChanged;
            _networkManager.ConnectionRequested += NetworkManagerConnectionRequested;
        }

        private void NetworkManagerOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NetworkManager.Message))
            {
                OnPropertyChanged(nameof(ChatText));
            }
        }
        private void NetworkManagerConnectionRequested(object? sender, ConnectionRequestedEventArgs e)
        {
            //Async
            Application.Current.Dispatcher.BeginInvoke(new Action (() =>
            {
                var a = new AcceptRequestWindow();
                a.ShowDialog();
                _networkManager.Message += "Connection Requested form: " + e.RemoteEndPoint;

            }));
            
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public ICommand SendMessageButtonCommand
        {
            get
            {
                if (sendMessageButtonCommand == null)
                {
                    sendMessageButtonCommand = new SendMessageCommand(this);
                }
                return sendMessageButtonCommand;

            }
            set
            {
                sendMessageButtonCommand = value;
            }
        }
        public ICommand SetNameUserCommand
        {
            get
            {
                if (setNameUserCommand == null)
                {
                    setNameUserCommand = new SetNameCommand(this);
                }
                return setNameUserCommand;
            }
            set
            {
                setNameUserCommand = value;
            }
        }
        public ICommand SetFriendPortCommand
        {
            get
            {
                if (setFriendPortCommand == null)
                {
                    setFriendPortCommand = new SetFriendPortCommand(this);
                }
                return setFriendPortCommand;
            }
            set
            {
                SetFriendPortCommand = value;
            }
        }
        public ICommand StartServerCommand
        {
            get
            {
                if (startServerCommand == null)
                {
                    startServerCommand = new StartServerCommand(this);
                }
                return startServerCommand;
            }
            set
            {
                startServerCommand = value;
            }
        }

        public void sendMessage()
        {
            _networkManager.Message = sendMessageTextBox;
            Debug.WriteLine(sendMessageTextBox);
        }

        public void setPortUser()
        {
            _networkManager.adress += portVm;
            Debug.WriteLine(_networkManager.adress);
            _networkManager.Port = portVm;
            _networkManager.Message += portVm;
            Debug.WriteLine("Port: " + portVm);

        }

        public void setNameUser()
        {
            _networkManager.Name = nameUser;
            Debug.WriteLine(nameUser);
        }
        public void setPortFriend()
        {
            _networkManager.FriendPort = friendPort;
            _networkManager.connectToFriend();
        }
        public void StartServer()
        {
            _networkManager.Message += "start connection in mainwindowviewmodel \n";
            _networkManager.startConnection();
        }
    }
}

