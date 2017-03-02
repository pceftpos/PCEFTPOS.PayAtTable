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

namespace PayAtTable.TestPos.IPInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            myEftWrapper.OnLogUpdate += MyEftWrapper_OnLogUpdate;
        }

        private void MyEftWrapper_OnLogUpdate(object sender, EventArgs e)
        {
            lvLog.SelectedIndex = lvLog.Items.Count;
            lvLog.ScrollIntoView(lvLog.SelectedItem);
        }

        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (myEftWrapper.Data.IsConnected)
            {
                myEftWrapper.Disconnect();
            }
            else
            {
                await myEftWrapper.Connect();
            }
        }

        private void btnClearLogs_Click(object sender, RoutedEventArgs e)
        {
            myEftWrapper.Logs.Clear();
        }

        private async void btnStatus_Click(object sender, RoutedEventArgs e)
        {
            await myEftWrapper.GetStatus();
        }
    }
}
