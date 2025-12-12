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

        public event PropertyChangedEventHandler PropertyChanged;

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
        private String _chatLogUrl = "C:\\Users\\isahe131\\source\\repos\\ChatApp\\ChatApp\\bin\\Debug\\net7.0-windows\\ChattHistorik\\";
        //private String _chatLogUrl = "C:\\Users\\hedma\\OneDrive\\Dokument\\GitHub\\ChatApp\\ChatApp\\bin\\Debug\\net7.0-windows\\ChattHistorik\\";
        private String _hasPort;
        private String portVm;
        private String nameUser;
        private String friendPort;
        private String sendMessageTextBox;
        private String _searchQuery;
        private bool _connectedOrDisconnectedMVM => _networkManager.IsConnected;
        public string connectedStatusString => _connectedOrDisconnectedMVM ? "Connected" : "Disconnected";

     

        public MainWindowViewModel(NetworkManager networkManager)
        {
            _networkManager = networkManager;
            _networkManager.ConnectionRequested += NetworkManagerConnectionRequested;
            _networkManager.ConnectionStatusChanged += OnConnectionStatusChanged;
            _networkManager.MessageReceived += OnMessageReceived;

            Messages = new ObservableCollection<ChatMessage>();
            Files = new ObservableCollection<string>();
            MessagesFromHistory = new ObservableCollection<ChatMessage>();
            LoadFiles(_chatLogUrl);
            FilteredFiles = new ObservableCollection<string>(Files);
        }
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void OnMessageReceived(ChatMessage msg)
        {
            App.Current.Dispatcher.BeginInvoke(async () =>
            {
                if (msg.Message == "ping")
                {
                    await Shake();
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

        private void NetworkManagerConnectionRequested(object? sender, ConnectionRequestedEventArgs e)
        {

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

        //Skickar meddelanden från den ena till den andra. SendJson är det som för över meddelandet
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
        //Informerar användaren huruvida den accepterat eller rejectat en connect-request.
        //HasPort informerar den som klickar accept/reject och temp skickas med ett meddelande till den som försöker ansluta.
        public async Task informUserAcceptDecline(bool userInput)
        {
            string temp = null;
            //Den som skickar request ska få tillbaka när det är accepterat eller declineat i form av ett meddelande.
            if (userInput)
            {
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

        //När användaren klickar på ping knappen ska meddelandet ping skickas
        //Detta innebär alltså att om användaren skriver in ping i chatten sker samma sak
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
        //När användaren mottar "ping" så kommer den här funktionen att köras. 
        //Simulerar något slags skak...
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
        //När användaren klickar på set username knappen
        public void setNameUser()
        {
            _networkManager.Name = nameUser;
        }
        //När användaren klickar på knappen för Connect To Friend
        public async void setPortFriend()
        {
            _networkManager.FriendPort = friendPort;
            bool connected = await _networkManager.connectToFriendAsync();
            //Här sätts hasport till port not open ifall porten användaren försöker ansluta till ej är öppen
            //Funkar eftersom connecttofriendasync returnar en boolean beroende på om det gått bra att ansluta till porten eller ej
            if (!connected)
            {

                HasPort = "Port not open";
            }
            else
            {
                HasPort = "Open port found";
            }
        }
        //Visar chatten man har valt från historiken
        public void displayChatHistory(string s)
        {
            //Rensar MessageFromHistory vilket är en observable collection som vi sparar den aktuella chattens meddelande i
            MessagesFromHistory.Clear();
            string json = $"{_chatLogUrl}{s}";
            string jsonString = File.ReadAllText(json);
            //Läser helt enkelt ut JSON och delar upp det i olika meddelanden
            //Visar sedan meddalanden genom att lägga till varje enstaka meddelanden i vår observable collection
            var msg = JsonSerializer.Deserialize<List<ChatMessage>>(jsonString);
            if (msg != null)
            {
                foreach (var ms in msg)
                {
                    MessagesFromHistory.Add(ms);
                }
            }
        }
        //När användaren klickar på set portnumber
        public void StartServer()
        {
            _networkManager.Port = portVm;
            _networkManager.startConnection();
        }

        //När användaren klickar på disconnectknappen
        public void DisconnectMVW()
        {
            if (_networkManager.IsConnected)
            {
                _networkManager?.Disconnect();
            }
        }
        //Funktion för att spara. Körs när användarens connectionstatus ändras med en onpropertychanged.
        //I propertychanged finns det en koll så att denna bara körs ifall användaren för nuvarande har en connection. 
        public void SaveChat()
        {
            string directoryString = "ChattHistorik";
            Directory.CreateDirectory(directoryString);
            //Så att den sparar från "servern",
            //det är bara den som faktiskt har ett friendname sparat tack vare att det skickas med acceptrutan. (Det behövs för att kunna spara båda namnen på användarna i filnamnet)
            //Smart lösning
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
        //Sökfunktionen i chathistoriken. Ganska straightforward. Kollar om filnamnen (strängarna) innehåller det användaren
        //skriver sökt efter. Sätter båda till gemener för att det inte ska bli några case-sensitivity problem
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
        //Laddar in alla filer från historiken. Kör en gång när programmet startar, sedan när användaren klickar på updatehistory

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
        //Denna tar över alla filer från files och kör in den i filteredfiles. Filtered files är den som visas på skärmen.
        //Gör detta för att Files har alla filer men filteredfiles utgår från files men visar endast de som matchar filtreringen(sökningen)
        public void copyOverFiles()
        {
            FilteredFiles.Clear();
            foreach (var file in Files)
            {
                FilteredFiles.Add(Path.GetFileName(file));
            }
        }

        //Nedan kommer alla ICommands och sedan getters och setters. Alla commands har en fil i
        //Viewmodel/commands. Den har i sin tur en eller fler funktioner i MainWindowViewModel som körs -
        //T.ex har updatehistorycommand
        //
        // parent.LoadFiles(parent.ChatLogUrl);
        // parent.copyOverFiles();
        /// 
        /// 
        /// 
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

