using ChatApp.Model;
using ChatApp.ViewModel;
using System.Windows;

namespace ChatApp
{
    public partial class AcceptRequestWindow : Window
    {
        //Behövdes göras till internal för att kunna ta in networkManager. Nu fungerar det. 
        private NetworkManager _networkManager;
        internal AcceptRequestWindow(NetworkManager networkManager)
        {
            _networkManager = networkManager;
            InitializeComponent();
            var vm = new AcceptRequestWindowViewModel(networkManager);
            DataContext = vm;

            vm.CloseAction = () => this.Close();
        }
    }
}
