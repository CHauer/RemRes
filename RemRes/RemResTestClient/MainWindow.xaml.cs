using System;
using System.Collections.Generic;
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
using RemResClientLib.Network.Notification;
using RemResDataLib.Messages;
using RemResTestClient.ViewModel;

namespace RemResTestClient
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            if (mainGrid != null && mainGrid.DataContext != null)
            {
                var mainViewModel = mainGrid.DataContext as MainViewModel;
                if (mainViewModel != null)
                {
                    viewModel = mainViewModel;
                    viewModel.NotificationReceived += MainWindow_NotificationReceived;
                }
            }
        }

        private void MainWindow_NotificationReceived(RemResMessage notificationMessage)
        {
            if (notificationMessage is Notification)
            {
                Notification msg = (notificationMessage as Notification);
                MessageBox.Show(String.Format(
                    "Notification\n"+
                    "{0}\n{1}", msg.Message, msg.ToString() 
                    ), "Notification received");
            }
        }


        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            if (viewModel != null){
                viewModel.StopNotificationEndpoint();
            }
        }
    }
}
