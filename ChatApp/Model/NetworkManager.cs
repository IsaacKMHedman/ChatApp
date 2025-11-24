using ChatApp.ViewModel.Commands;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
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
        string tessss;
        private string a;
        private string test;
        private NetworkStream stream;
        public string adress = "127.0.0.1:";
        string _Port = "";
        string _Name = "";
        string _Message = "";
        string _FriendPort = "";
        TcpListener server;
        TcpClient endPoint;

        //Konstruktorn kanske inte behövs
        public NetworkManager()
        {

        }
        //Något sånt här? För att få en async funktion... Testade detta också endpoint blev TcpClient endPoint = async wait server.AcceptTcpClientAsync() Men då sprängdes programmet.
        //Den väntade på något...
        //Denna ersatt alltså startConnection under
        //public async Task startConnection()
        //{

        //}
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
                    runWhenListenerGotConnection(endPoint);
                    handleConnection(endPoint);
                }
                catch
                {
                    Message += "Catchar efter först try ... \n";
                }
            });
            
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

        //Async task? Kanske inte
        //Den här funktionen ska förhoppningsvis köra någonting i mainwindowviewmodel som i sin tur öppnar upp accept rutan som i sin tur skickar tillbaka till viewmodel vad man svara som skickar tillbaka hit
        // Alltså : Model -> ViewModel -> View -> ViewModel -> ViewModel (Command) -> ViewModel -> Model. Hur gör man? 
        public void runWhenListenerGotConnection(TcpClient endPoint)
        {
            stream = endPoint.GetStream();
            Message += "Inne i runwhenlistenergotconnection \n";

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
