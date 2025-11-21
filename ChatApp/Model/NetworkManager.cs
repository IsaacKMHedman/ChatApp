using ChatApp.ViewModel.Commands;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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
        private NetworkStream stream;
        public string adress = "127.0.0.1:";
        string _Port = "";
        string _Name = "";
        string _Message = "";
        string _FriendPort = "";
        //Kanske en boolean som sätts till true om man får en connection

        //Demot har ingen public networkmanager
        public NetworkManager() {

        }

        public bool startConnection()
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
                    runWhenListenerGotConnection();
                    handleConnection(endPoint);
                }
                catch
                {
                    Message += "Catchar efter först try ... \n";
                }
            });
            return true;
        }
        public bool connectToFriend()
        {
            Task.Factory.StartNew(() =>
            {
                TcpClient endPoint = new TcpClient();

                try
                {
                    //@TODO här måste man kolla så att det finns en på den porten, hittas den inte ska det avbrytas..
                    Message += "Looking for port " + _FriendPort;
                    Message += "Connecting to the server... ";
                    endPoint.Connect(IPAddress.Loopback, int.Parse(_FriendPort));
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

        private void runWhenListenerGotConnection()
        {

        }
        private void handleConnection(TcpClient endPoint)
        {
            stream = endPoint.GetStream();
            Message += "Inne i Handle connection ... \n ";
            while (true)
            {
                var buffer = new byte[1024];
                int received = stream.Read(buffer, 0, 1024);
                var message = Encoding.UTF8.GetString(buffer);
                this.Message = message;
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
            get { return _FriendPort; }
            set
            {
                _FriendPort = value;
                OnPropertyChanged();
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

    }
}
