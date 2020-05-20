using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Multi_Thread_Web_Server;

namespace ServerViewApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Server server;
        UIConsole uiConsole = new UIConsole();
        public ObservableCollection<RequestResponseViewModel> observable = new ObservableCollection<RequestResponseViewModel>();
        AppIsolatedStorage storage = new AppIsolatedStorage();

        public MainWindow()
        {
            this.DataContext = observable;
            InitializeComponent();
            logListView.ItemsSource = uiConsole.Logs;

            BindingOperations.EnableCollectionSynchronization(observable, new object { });
            BindingOperations.EnableCollectionSynchronization(uiConsole.Logs, new object { });
            
            try
            {
                int port = storage.GetPortForUser();
                if(port > 0)
                {
                    portTxtBox.Text = port.ToString();
                } 
            }
            catch(Exception)
            {
                // do nothing...
            }
        }

        private void ServedRequests_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            observable.Clear();
            foreach(KeyValuePair<IRoute, IResponse> kv in (sender as ObservableCollection<KeyValuePair<IRoute, IResponse>>))
            {
                RequestResponseViewModel vm = new RequestResponseViewModel();
                vm.Method = kv.Key.Method;
                vm.Path = kv.Key.Path;
                vm.StatusCode = kv.Value.StatusCode;
                vm.Time = kv.Key.Time.ToString();

                observable.Add(vm);
            }
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            string s = portTxtBox.Text;
            int port;

            if(int.TryParse(s, out port))
            {
                this.server = new Server(4, port, uiConsole);
                server.servedRequests.CollectionChanged += ServedRequests_CollectionChanged;
                this.server.Listen();
                this.startBtn.IsEnabled = false;
                this.stopBtn.IsEnabled = true;
            } 
            else
            {
                MessageBox.Show("You need to enter a port number to start this server.", "Port not specified.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            this.server.StopAllThreads();
            MessageBox.Show("Server stopped successfully", "Server Stopped", MessageBoxButton.OK, MessageBoxImage.Information);

            this.startBtn.IsEnabled = true;
            this.stopBtn.IsEnabled = false;
        }

        private void btnSaveLogs_Click(object sender, RoutedEventArgs e)
        {
            List<string> logs = uiConsole.Logs.ToList();
            storage.SaveLogs(logs);               
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string str = portTxtBox.Text;
            storage.SavePortToUser(str);
            Environment.Exit(0);
        }
    }
}
