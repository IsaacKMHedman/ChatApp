using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatApp.Model
{
    internal class NeworkManager : INotifyPropertyChanged
    {
        private NetworkStream stream;

        public event PropertyChangedEventHandler PropertyChanged;


        private void OnPropertyChanged(string propertyName="")
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private string message;
        public string Message
        {
            get { return message; }
            set { message = value; OnPropertyChanged("Message"); }
        }



        public bool startConnection()
        {

            Task.Factory.StartNew(() =>
            {
                bool secondTry = false;
                var ipEndpoint = new IPEndPoint(IPAddress.Loopback, 1000); //Localhost : 1000?
                TcpListener server = new TcpListener(ipEndpoint); //Skapar lyssnaren
                TcpClient endPoint = null; //Det här borde vara den som skickar 

                try
                {
                    server.Start();
                    System.Diagnostics.Debug.WriteLine("Starting the listening");
                    endPoint = server.AcceptTcpClient();
                    System.Diagnostics.Debug.WriteLine("Connection accepted!!");
                    handleConnection(endPoint);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    secondTry = true;
                }

                if (secondTry)
                {
                    endPoint = new TcpClient();
                }
                    try
                    {
                        server.Start();
                        System.Diagnostics.Debug.WriteLine("Starting the listening");
                        endPoint = server.AcceptTcpClient();
                        System.Diagnostics.Debug.WriteLine("Connection accepted!!");
                        handleConnection(endPoint);
                    }finally
                    {
                        endPoint.Close();
                    }
            });

            return true;

        }

        private void handleConnection(TcpClient endPoint)
        {
            stream = endPoint.GetStream();
            while (true)
            {
                var buffer = new byte[1024];
                int received = stream.Read(buffer, 0, 1024);
                var message = Encoding.UTF8.GetString(buffer, 0, received);
                this.Message = message;
            }
        }
        public void sendChar(string str)
        {
            Task.Factory.StartNew(() => {
                var buffer = Encoding.UTF8.GetBytes(str);
                stream.Write(buffer, 0, buffer.Length);
                });
        }
    }
}
