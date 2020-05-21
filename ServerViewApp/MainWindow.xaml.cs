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

        /// <summary>
        /// Main Window Consturctor. 
        /// It initialises the data context used for bindings of the request list view.
        /// It also enables binding synchronization in order to allow bound object to be
        /// modified by other threads.
        /// On start it attempts to grab the port number from the user settings saved in isolated storage.
        /// </summary>
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

        /// <summary>
        /// Method used when the served requests observable in the server has changed.
        /// It updated the observable bound to the GUI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// When the start button is clicked, the server is recreated and started.
        /// The start btn performs a check to ensure that the port entered is an actual number and that it is over 1000.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            string s = portTxtBox.Text;
            int port = -1;

            if(int.TryParse(s, out port) && port > 1000)
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

        /// <summary>
        /// When the stop button is clicked, the server threads are aborted and the buttons are reenabled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            this.server.StopAllThreads();
            MessageBox.Show("Server stopped successfully", "Server Stopped", MessageBoxButton.OK, MessageBoxImage.Information);

            this.startBtn.IsEnabled = true;
            this.stopBtn.IsEnabled = false;
        }

        /// <summary>
        /// When the save logs button is clicked, a snapshot of all logs appearing on screen is saved
        /// and they are persisted in the isolated storage.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveLogs_Click(object sender, RoutedEventArgs e)
        {
            List<string> logs = uiConsole.Logs.ToList();
            storage.SaveLogs(logs);
            MessageBox.Show("Logs saved successfully!", "Logs Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// When the window is being closed, the port number is saved to isolated storage and the cleanup
        /// procedure is started.
        /// In the end the application closes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string str = portTxtBox.Text;
            storage.SavePortToUser(str);
            server.StopAllThreads();
            Environment.Exit(0);
        }
    }
}
