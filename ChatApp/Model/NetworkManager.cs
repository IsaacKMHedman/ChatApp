using ChatApp.ViewModel.Commands;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.TextFormatting;
using System.Xml.Schema;

namespace ChatApp.Model
{
    internal class NetworkManager : INotifyPropertyChanged
    {

        private TaskCompletionSource<bool> _waitForConnectDecision;
        public event EventHandler<ConnectionRequestedEventArgs>? ConnectionRequested;
        private NetworkStream stream;
        public string adress = "127.0.0.1:";
        string _Port = "";
        string _Name = "";
        string _Message = "";
        string _friendPort = "";
        string _friendName = "";



        //Konstruktorn kanske inte behövs
        public NetworkManager()
        {

        }

        //Det vi vill här (Om jag tolkar det rätt... runWhenListenerGotConnection ska...)
        // : Model -> ViewModel -> View -> ViewModel -> ViewModel(Command) -> ViewModel -> Model. Hur gör man?
        public void startConnection()
        {

            Task.Factory.StartNew(() =>
            {
                TcpListener server = new TcpListener(IPAddress.Loopback, int.Parse(_Port));
                TcpClient endPoint = null;

                try
                {
                    Message += "Inne i first try... ";
                    server.Start();
                    Message += "Starting the listening ... ";
                    //Den här sitter och väntar tills någon kopplar upp sig
                    endPoint = server.AcceptTcpClient();
                    Message += "Connection accepted \n";
                    var tcs = new TaskCompletionSource<bool>();
                    _waitForConnectDecision = tcs;

                    runWhenListenerGotConnection(endPoint);

                    //Väntar på att användaren ska klicka accept/decline
                    bool accepted = tcs.Task.Result; //Blockerar endast denna background

                    if (accepted)
                    {
                        Message += "Accepted \n";
                        handleConnection(endPoint);

                    }
                    else
                    {
                        Message += "Declined";
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
        public bool connectToFriend()
        {
            Task.Factory.StartNew(() =>
            {
                TcpClient endPoint = new TcpClient();

                try
                {
                    //@TODO här måste man kolla så att det finns en på den porten, hittas den inte ska det avbrytas..
                    Message += "Looking for port " + _friendPort;
                    Message += "Connecting to the server... ";
                    endPoint.Connect(IPAddress.Loopback, int.Parse(_friendPort));
                    stream = endPoint.GetStream();
                    using var sender = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
                    sender.Write(Name);
                    sender.Write(Port);
                    sender.Flush();

                    Message += "Connection established \n";
                    handleConnection(endPoint);
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
        private void handleConnection(TcpClient endPoint)
        {
            stream = endPoint.GetStream();
            Message += "Inne i Handle connection ... \n ";
            while (true)
            {
                var buffer = new byte[1024];
                int received = stream.Read(buffer, 0, 1024);
                if (received == 0)
                {
                    Message += "Anslutning avslutad \n";
                    break;
                }
                var message = Encoding.UTF8.GetString(buffer);
                this.Message += _friendName + ": " + message;
            }
        }

        public void AcceptConnection()
        {
            Message += "Inne i AcceptConnection \n ";
            _waitForConnectDecision?.TrySetResult(true);
        }
        public void RejectConnection()
        {
            Message += "Inne i rejectconnection \n ";
            _waitForConnectDecision?.TrySetResult(false);
        }

        public void SendChatMessage(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                var temp = text + "\n";
                var bytes = Encoding.UTF8.GetBytes(temp);
                stream.Write(bytes, 0, bytes.Length);
            }
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
