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
using System.Text.Json;

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
        private ICommand disconnectCommand;
        public ObservableCollection<ChatMessage> Messages { get; set; }

        //Det här är tänkt så att man ska kunna ändra texten i chatrutan. 
        public String ChatText => _networkManager.Message;

        private String portVm;
        private String nameUser;
        private String friendPort;
        private String sendMessageTextBox;
        public string PortVm
        {
            get => portVm;
            set
            {
                portVm = value; OnPropertyChanged(nameof(PortVm));
            }
        }

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

            Messages = new ObservableCollection<ChatMessage>();
            _networkManager.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(ChatMessage msg)
        {
            App.Current.Dispatcher.BeginInvoke(() =>
            {
                Messages.Add(msg);
            });
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
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var a = new AcceptRequestWindow();
                var vm = new AcceptRequestWindowViewModel(_networkManager);
                a.DataContext = vm;

                vm.CloseAction += () => a.Close();
                vm.AcceptAction += async () => await informUserAcceptDecline(true);
                vm.RejectAction += async () => await informUserAcceptDecline(false);

                a.ShowDialog();


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
        public ICommand DisconnectCommand { 
            get
            {
                if (disconnectCommand == null)
                {
                    disconnectCommand = new DisconnectCommand(this);
                }
                return disconnectCommand;
            }
            set
            {
                disconnectCommand = value;
            }
        }

        public async Task sendMessageAsync()
        {
            var msg = new ChatMessage
            {
                Name = NameUser,
                Message = sendMessageTextBox,
                Date = DateTime.Now
            };

            Messages.Add(msg);
            sendMessageTextBox = "";
            await _networkManager.SendJson(msg);

        }
        public async Task informUserAcceptDecline(bool userInput)
        {
            string temp = null;
            //Samma sak på rad kan kanske bryta ut till en funktion om man vill, risk för mer oläslighet
            if (userInput)
            {
                temp = "Accepted Request...";
                
            }
            else
            {
                temp = "Rejected request";
            }
            var msg = new ChatMessage
            {
                Name = NameUser,
                Message = temp,
                Date = DateTime.Now

            };
            await _networkManager.SendJson(msg);
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
            _networkManager.startConnection();
        }

        public void DisconnectMVW()
        {
            var msg = new ChatMessage
            {
                Name = NameUser,
                Message = "Disconnected!!!",
                Date = DateTime.Now
            };
            _networkManager?.SendJson(msg);
            _networkManager?.Disconnect();
        }
    }
}

