using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ChatApp.Model
{
    internal class NetworkManager : INotifyPropertyChanged
    {
        //@CHANGED -- Ändrade ordningen på dessa så de är lite grupperade 
        public event Action<ChatMessage> MessageReceived;
        public event EventHandler<ConnectionRequestedEventArgs>? ConnectionRequested;
        public event EventHandler ConnectionStatusChanged;

        private CancellationTokenSource? _waitForDisconnect;
        private TaskCompletionSource<bool> _waitForConnectDecision;
        
        private NetworkStream stream;
        private StreamWriter writer;
        private StreamReader reader;
        private TcpClient endPoint;

        //@CHANGED -- Ändrade dessa under till private
        private string adress = "127.0.0.1:";
        private string _Port = "";
        private string _Name = "";
        //string _Message = "";
        private string _friendPort = "";
        private string _friendName = "";
        private bool isConnected = false;

        //Startconnection körs när man sätter sin port
        public void startConnection()
        {

            Task.Factory.StartNew(() =>
            {
                TcpListener server = new TcpListener(IPAddress.Loopback, int.Parse(_Port));
                endPoint = null;

                try
                {
                    server.Start();
                    //Den här sitter och väntar tills någon kopplar upp sig
                    endPoint = server.AcceptTcpClient();
                    reader = new StreamReader(endPoint.GetStream());
                    writer = new StreamWriter(endPoint.GetStream());

                    var tcs = new TaskCompletionSource<bool>();
                    _waitForConnectDecision = tcs;

                    runWhenListenerGotConnection(endPoint);


                    //Väntar på att användaren ska klicka accept/decline
                    bool accepted = tcs.Task.Result; //Blockerar endast denna background

                    if (accepted)
                    {

                        _ = ListenForMessages(reader);
                        //handleConnection(endPoint);

                    }
                    else
                    {
                        endPoint.Close();
                    }
                }
                catch
                {
                    //Message += "Catchar efter först try ... \n";
                }
            });

        }

        //Här är där man connectar som endpoint. Här skickar vi även med namnet på den som försöker ansluta.
        //Connect to friend kör listenformessages för snabbt. Den kör den utan att kolla om den andra faktiskt har accepterat.
        public async Task<bool> connectToFriendAsync()
        {
            //Task.Factory.StartNew(async () =>
            //{
                TcpClient endPoint = new TcpClient();

                try
                {
                //TryParse fungerar bara med out var port, den skapar tydligen en variabel och lägger resultatet där
                //Om det är en tom sträng kraschar programmet, om så denna koll är väldigtväldigt viktig
                    if (!int.TryParse(_friendPort, out var port))
                    {
                        return false;
                    }
                    await endPoint.ConnectAsync(IPAddress.Loopback, int.Parse(_friendPort));
                    stream = endPoint.GetStream();
                    reader = new StreamReader(stream);
                    writer = new StreamWriter(stream);

                    //Det här skickar bara med namn och så i acceptrutan
                    using var sender = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
                    sender.Write(Name);
                    sender.Write(Port);
                    sender.Flush();

                    _ = ListenForMessages(reader);

                     return true;

                }
                catch (SocketException)
                {
                    return false;
                }
                finally
                {
                    //@REMOVE - Ta inte bort förrän vi tar reda på vad gör finally egentligen? 
                    //endPoint.Close();
                }

            //});
            //return true;
        }

        //Okej såhär är det. Listenern får namnet på den som försöker. Därför har vi det.
        //Vi måste skicka tillbaka namnet på listenern till den andre
        public void runWhenListenerGotConnection(TcpClient endPoint)
        {
            stream = endPoint.GetStream();
            using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
            FriendName = reader.ReadString();
            FriendPort = reader.ReadString();

            if (endPoint.Client.RemoteEndPoint is IPEndPoint remoteEndPoint)
            {
                OnConnectionRequested(new ConnectionRequestedEventArgs(remoteEndPoint));
            }
        }

        public void AcceptConnection()
        {

            _waitForConnectDecision?.TrySetResult(true);
        }
        public void RejectConnection()
        {
            _waitForConnectDecision?.TrySetResult(false);
        }

        public async Task SendJson(ChatMessage msg)
        {
            string json = JsonSerializer.Serialize(msg);
            await writer.WriteLineAsync(json);
            await writer.FlushAsync();
        }

        private async Task ListenForMessages(StreamReader reader)
        {
            isConnected = true;
            ConnectionStatusChanged?.Invoke(this, EventArgs.Empty);
            _waitForDisconnect = new CancellationTokenSource();
            var token = _waitForDisconnect.Token;


            while (!token.IsCancellationRequested)
            {
                string? json = await reader.ReadLineAsync(token);
                if (json == null)
                {
                    break;
                }
                var msg = JsonSerializer.Deserialize<ChatMessage>(json);
                if (msg != null)
                {
                    MessageReceived?.Invoke(msg);
                }
            }
            isConnected = false;
            ConnectionStatusChanged?.Invoke(this, EventArgs.Empty);

        }

        public void Disconnect() {

            if (isConnected)
            {
                _waitForDisconnect?.Cancel();
                stream?.Close();
                endPoint?.Close();
                isConnected = false;
                ConnectionStatusChanged?.Invoke(this, EventArgs.Empty);
            }

        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        protected void OnConnectionRequested(ConnectionRequestedEventArgs e)
        {
            ConnectionRequested?.Invoke(this, e);
        }
        public string Port
        {
            get { return _Port; }
            set
            {
                _Port = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                OnPropertyChanged();
            }

        }

        //@REMOVE
        //public string Message
        //{
        //    get { return _Message; }
        //    set
        //    {
        //        _Message = value;
        //        OnPropertyChanged();
        //    }
        //}

        public string FriendPort
        {
            get { return _friendPort; }
            set
            {
                _friendPort = value;
                OnPropertyChanged();
            }
        }


        public bool IsConnected
        {
            get { return isConnected; }
            set
            {
                isConnected = value;
                OnPropertyChanged();
            }
        }

        public string FriendName
        {
            get => _friendName;
            set
            {
                if (_friendName != value)
                {
                    _friendName = value;
                    OnPropertyChanged(nameof(FriendName));
                }
            }
        }

    }
    internal class ConnectionRequestedEventArgs : EventArgs
    {
        public ConnectionRequestedEventArgs(IPEndPoint remoteEndPoint)
        {
            RemoteEndPoint = remoteEndPoint;
        }

        public IPEndPoint RemoteEndPoint { get; }
    }

}
