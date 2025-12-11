using ChatApp.Model;
using ChatApp.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Text.Json;
using System.IO;
using System.Printing;
using System.Security.Cryptography.X509Certificates;

namespace ChatApp.ViewModel
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {

        private NetworkManager _networkManager { get; set; }

        private ICommand sendMessageButtonCommand;
        private ICommand setNameUserCommand;
        private ICommand setFriendPortCommand;
        private ICommand startServerCommand;
        private ICommand disconnectCommand;
        private ICommand updateChatHistoryCommand;
        private ICommand searchHistoryCommand;
        private ICommand pingCommand;

        public ObservableCollection<ChatMessage> Messages { get; set; }
        public ObservableCollection<string> Files { get; set; }
        public ObservableCollection<ChatMessage> MessagesFromHistory { get; set; }
        public ObservableCollection<string> FilteredFiles {  get; set; }

        private string _selectedChatHistory;
        private int _windowHeight = 900;
        private int _windowWidth = 800;
        //Det här är tänkt så att man ska kunna ändra texten i chatrutan. 
        //public String ChatText => _networkManager.Message;
        //public String chatLogUrl = "C:\\Users\\isahe131\\source\\repos\\ChatApp\\ChatApp\\bin\\Debug\\net7.0-windows\\ChattHistorik\\";
        //@CHANGED -- Ändrade chatlogurl till private, gjorde en getter istället. Den användes i updatechatcommand
        private String _chatLogUrl = "C:\\Users\\hedma\\OneDrive\\Dokument\\GitHub\\ChatApp\\ChatApp\\bin\\Debug\\net7.0-windows\\ChattHistorik\\";
        private String _hasPort;
        private String portVm;
        private String nameUser;
        private String friendPort;
        private String sendMessageTextBox;
        private String _searchQuery;
        private bool _connectedOrDisconnectedMVM => _networkManager.IsConnected;
        //@CHANGED -- Försökte ändra connectedstatusstring till private - Fick det inte att funka
        //Vet inte varför
        public string connectedStatusString => _connectedOrDisconnectedMVM ? "Connected" : "Disconnected";

        //@CHANGED Flyttade ner samtliga get/set nederst

        public MainWindowViewModel(NetworkManager networkManager)
        {
            _networkManager = networkManager;
            //@CHANGED -- Eftersom message i networkmanager inte används kan vi nog ta bort denna rad
            //_networkManager.PropertyChanged += NetworkManagerOnPropertyChanged;
            _networkManager.ConnectionRequested += NetworkManagerConnectionRequested;
            _networkManager.ConnectionStatusChanged += OnConnectionStatusChanged;
            Messages = new ObservableCollection<ChatMessage>();
            Files = new ObservableCollection<string>();
            MessagesFromHistory = new ObservableCollection<ChatMessage>();
            LoadFiles(_chatLogUrl);
            FilteredFiles = new ObservableCollection<string>(Files);
            _networkManager.MessageReceived += OnMessageReceived;

        }

        //@CHANGED -- Flyttade ner funktioner härifrån. Här är våra onpropertychanged först
        private void OnMessageReceived(ChatMessage msg)
        {
            App.Current.Dispatcher.BeginInvoke(() =>
            {
                if (msg.Message == "ping")
                {
                    Shake();
                }
                else
                {
                    Messages.Add(msg);
                }
            });
        }
        
        private void OnConnectionStatusChanged(object sender, EventArgs e)
        {
            //Det här är så den bara sparar när den faktiskt har en connection och inte när den först skapar en connection
            if (!_connectedOrDisconnectedMVM)
            {
                SaveChat();
            }
            //Här sätts hasport till ingenting bara för att det ska vara tydligt för användaren
            HasPort = "";
            OnPropertyChanged(nameof(_connectedOrDisconnectedMVM));
            OnPropertyChanged(nameof(connectedStatusString));
        }

        //Chattext tror jag inte används...
        //private void NetworkManagerOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == nameof(NetworkManager.Message))
        //    {
        //        OnPropertyChanged(nameof(ChatText));
        //    }
        //}
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
        //@CHANGED -- Flyttade ner alla ICommand och onpropertychanged
        //@CHANGED -- Flyttade ner alla funktioner, har alla tasks överst sen kommer funktioner
        public async Task sendMessageAsync()
        {
            if(sendMessageTextBox != "")
            {
                var msg = new ChatMessage
                {
                    Name = NameUser,
                    Message = sendMessageTextBox,
                    Date = DateTime.Now
                };
                Messages.Add(msg);
                sendMessageTextBox = "";
                OnPropertyChanged(sendMessageTextBox);
                await _networkManager.SendJson(msg);
            }
        }
        public async Task informUserAcceptDecline(bool userInput)
        {
            string temp = null;
            //Den som skickar request ska få tillbaka när det är accepterat eller declineat i form av ett meddelande.
            if (userInput)
            {
                //Okej man hade kunnat haft någonting som lyssnar och ändrar ens hasport beroende på något i networkmanager. 4
                //Däremot så kan det lika gärna skickas som ett meddelande
                HasPort = "Accepted Request...";
                temp = "Accepted Request...";
            }
            else
            {
                HasPort = "Rejected request";
                temp = "Rejected request";
            }
            OnPropertyChanged(nameof(HasPort));
            var msg = new ChatMessage
            {
                Name = NameUser,
                Message = temp,
                Date = DateTime.Now

            };
            await _networkManager.SendJson(msg);
        }


        public async Task pingFriendAsync()
        {
            ChatMessage msg = new ChatMessage
            {
                Name = NameUser,
                Message = "ping",
                Date = DateTime.Now
            };
            await _networkManager.SendJson(msg);
        }
        async Task Shake()
        {
            int shake = 10;
            for (int i = 0; i < 5; i++)
            {
                WindowHeight = WindowHeight + shake;
                WindowWidth = WindowWidth + shake;
                await Task.Delay(100);
            }
            for (int i = 0; i < 5; i++)
            {
                WindowHeight = WindowHeight - shake;
                WindowWidth = WindowWidth - shake;
                await Task.Delay(100);
            }
            for (int i = 0; i < 5; i++)
            {
                WindowHeight = WindowHeight + shake;
                WindowWidth = WindowWidth + shake;
                await Task.Delay(100);
            }
            for (int i = 0; i < 5; i++)
            {
                WindowHeight = WindowHeight - shake;
                WindowWidth = WindowWidth - shake;
                await Task.Delay(100);
            }
            //Utan det här kan man spamma och förstöra dimensionerna på den andras chat.
            WindowHeight = 900;
            WindowWidth = 800;
        }
        public void setNameUser()
        {
            _networkManager.Name = nameUser;
            Debug.WriteLine(nameUser);
        }
        public async void setPortFriend()
        {
            _networkManager.FriendPort = friendPort;
            bool connected = await _networkManager.connectToFriendAsync();
            if (!connected)
            {

                HasPort = "Port not open";
            }
            else
            {
                HasPort = "Open port found";
            }
        }
        public void displayChatHistory(string s)
        {
            MessagesFromHistory.Clear();
            string json = $"{_chatLogUrl}{s}";
            string jsonString = File.ReadAllText(json);
            var msg = JsonSerializer.Deserialize<List<ChatMessage>>(jsonString);
            if (msg != null)
            {
                foreach (var ms in msg)
                {
                    MessagesFromHistory.Add(ms);
                }
            }
        }
        public void StartServer()
        {
            _networkManager.Port = portVm;
            _networkManager.startConnection();
        }

        public void DisconnectMVW()
        {
            if (_networkManager.IsConnected)
            {
                //SaveChat();
                _networkManager?.Disconnect();
            }
        }
        public void SaveChat()
        {
            string directoryString = "ChattHistorik";
            Directory.CreateDirectory(directoryString);
            //Så att den sparar från "servern", den som faktiskt har ett friendname sparat tack vare att det skickas med acceptrutan. Smart lösning
            if (_networkManager.FriendName != "")
            {
                string filename = $"{DateTime.Now:yyyy-MM-dd_HHmmss}_{nameUser}_{_networkManager.FriendName}.json";
                string path = Path.Combine(directoryString, filename);
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(Messages, options);

                File.WriteAllText(path, json);
            }
        }
        public void searchInHistory()
        {
            var q = SearchQuery.ToLower();
            var results = Files.Where(x => x.ToLower().Contains(q)).ToList();

            FilteredFiles.Clear();
            foreach (var r in results)
            {
                FilteredFiles.Add(r);
            }

        }
        public void LoadFiles(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return;

            Files.Clear();

            foreach (var file in Directory.GetFiles(folderPath))
            {
                Files.Add(Path.GetFileName(file));
            }
        }
        public void copyOverFiles()
        {
            FilteredFiles.Clear();
            foreach (var file in Files)
            {
                FilteredFiles.Add(Path.GetFileName(file));
            }
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
        public ICommand SearchHistoryCommand
        {
            get
            {
                if (searchHistoryCommand == null)
                {
                    searchHistoryCommand = new SearchHistoryCommand(this);
                }
                return searchHistoryCommand;
            }
            set
            {
                searchHistoryCommand = value;

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
        public ICommand DisconnectCommand
        {
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

        public ICommand UpdateChatHistoryCommand
        {
            get
            {
                if (updateChatHistoryCommand == null)
                {
                    updateChatHistoryCommand = new UpdateChatHistoryCommand(this);
                }
                return updateChatHistoryCommand;
            }
            set
            {
                disconnectCommand = value;
            }
        }
        public ICommand PingCommand
        {
            get
            {
                if (pingCommand == null)
                {
                    pingCommand = new PingCommand(this);
                }
                return pingCommand;
            }
            set
            {
                disconnectCommand = value;
            }
        }
        public String ChatLogUrl
        {
            get => _chatLogUrl;
        }
        public String HasPort
        {
            get => _hasPort;
            set
            {
                _hasPort = value;
                OnPropertyChanged(nameof(HasPort));
            }
        }
        public int WindowHeight
        {
            get => _windowHeight;
            set
            {
                _windowHeight = value;
                OnPropertyChanged(nameof(WindowHeight));
            }
        }
        public int WindowWidth
        {
            get => _windowWidth;
            set
            {
                _windowWidth = value;
                OnPropertyChanged(nameof(WindowWidth));
            }
        }
        public string SelectedChatHistory
        {
            get => _selectedChatHistory;
            set
            {
                _selectedChatHistory = value; OnPropertyChanged(nameof(SelectedChatHistory));
                displayChatHistory(_selectedChatHistory);
            }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged(nameof(SearchQuery));
            }
        }
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
    }
}

