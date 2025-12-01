using ChatApp.Model;
using ChatApp.ViewModel;
using System.Net;
using System.Windows;

namespace ChatApp
{
    public partial class AcceptRequestWindow : Window
    {
        //Behövdes göras till internal för att kunna ta in networkManager. Nu fungerar det. 
        public AcceptRequestWindow()
        {
            InitializeComponent();   
        }
    }
}
