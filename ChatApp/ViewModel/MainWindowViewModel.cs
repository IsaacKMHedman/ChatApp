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

        
        private NetworkManager _networkManager;
        private ICommand sendMessageButtonCommand;
        private ICommand setPort;

        //Det här är tänkt så att man ska kunna ändra texten i chatrutan. 
        public String ChatText => _networkManager.FirstName;
        public String Port => _networkManager.Port;
        private String portVm;
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



        public MainWindowViewModel(NetworkManager networkManager)
        {
            
            _networkManager = networkManager;
            _networkManager.PropertyChanged += NetworkManagerOnPropertyChanged;

        }

        private void NetworkManagerOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(NetworkManager.FirstName))
            {
                OnPropertyChanged(nameof(ChatText));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public ICommand SetPort
        {
            get
            {
                if (setPort == null)
                {
                    setPort = new SetPort(this);
                }
                return setPort;
            }
            set
            {
                setPort = value;
            }
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
        public void sendMessage()
        {
            _networkManager.FirstName = sendMessageTextBox;
            Debug.WriteLine(sendMessageTextBox);
        }

        public void setPortUser()
        {
            _networkManager.adress += portVm;
            Debug.WriteLine(_networkManager.adress);
            _networkManager.Port = portVm;
            _networkManager.FirstName += portVm;
            Debug.WriteLine("Port: " + portVm);

        }
    }
}
