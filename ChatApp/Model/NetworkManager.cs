using System;
using System.ComponentModel;
using System.Diagnostics;
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
        public event Action<ChatMessage> MessageReceived;
        private CancellationTokenSource? _waitForDisconnect;
        private TaskCompletionSource<bool> _waitForConnectDecision;
        public event EventHandler<ConnectionRequestedEventArgs>? ConnectionRequested;
        public event EventHandler ConnectionStatusChanged;
        private bool isConnected = false;

        private NetworkStream stream;
        public string adress = "127.0.0.1:";
        string _Port = "";
        string _Name = "";
        string _Message = "";
        string _friendPort = "";
        string _friendName = "";
        private StreamWriter writer;
        private StreamReader reader;
        private TcpClient endPoint;

        public void startConnection()
        {

            Task.Factory.StartNew(() =>
            {
                TcpListener server = new TcpListener(IPAddress.Loopback, int.Parse(_Port));
                endPoint = null;

                try
                {
                    Message += "Inne i first try... ";
                    server.Start();
                    Message += "Starting the listening ... ";
                    //Den här sitter och väntar tills någon kopplar upp sig
                    endPoint = server.AcceptTcpClient();
                    Message += "Connection accepted \n";
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
                    Message += "Catchar efter först try ... \n";
                }
            });

        }

        //Här är där man connectar som endpoint. Här skickar vi även med namnet på den som försöker ansluta.
        //Connect to friend kör listenformessages för snabbt. Den kör den utan att kolla om den andra faktiskt har accepterat.
        public bool connectToFriend()
        {
            Task.Factory.StartNew(async () =>
            {
                TcpClient endPoint = new TcpClient();

                try
                {

                    //@TODO här måste man kolla så att det finns en på den porten, hittas den inte ska det avbrytas..
                    Message += "Looking for port " + _friendPort;
                    Message += "Connecting to the server... ";
                    endPoint.Connect(IPAddress.Loopback, int.Parse(_friendPort));
                    stream = endPoint.GetStream();
                    reader = new StreamReader(stream);
                    writer = new StreamWriter(stream);

                    //Det här skickar bara med namn och så i acceptrutan
                    using var sender = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
                    sender.Write(Name);
                    sender.Write(Port);
                    sender.Flush();

                    await ListenForMessages(reader);

                }
                finally
                {

                    endPoint.Close();
                }

            });
            return true;
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
       
            //Här kanske man ska stänga tcpendpoint och stream osv....
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

        public string Message
        {
            get { return _Message; }
            set
            {
                _Message = value;
                OnPropertyChanged();
            }
        }

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
