using AliceBlueWrapper;
using AliceBlueWrapper.Models;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Trading_App.Common;
using Trading_App.Model;
using Trading_App.Processor;

namespace Trading_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Variables
        private TradeSetting tradeSetting;
        private GmailConnect gmailConnect;
        private MTMConnect mtmConnect;
        APIProcessor apiProcessor;
        #endregion
       
        public MainWindow()
        {
            InitializeComponent();
            ReadSetting();
            CheckToken();            
            //SetGmailSetting();
            apiProcessor = new APIProcessor(tradeSetting);
            apiProcessor.LoadMasterContract();           
        }        
        private void ReadSetting()
        {
            tradeSetting = new TradeSetting
            {
                UserId = ConfigurationManager.AppSettings["UserId"],
                Password = ConfigurationManager.AppSettings["Password"],
                ClientSecret = ConfigurationManager.AppSettings["ClientSecret"],
                Token = ConfigurationManager.AppSettings["Token"],
                TokenCreatedOn = ConfigurationManager.AppSettings["TokenCreatedOn"],
                Quantity = ConfigurationManager.AppSettings["Quantity"],
                StopLossPercentage = ConfigurationManager.AppSettings["StopLossPercentage"],
                ExpiryWeek = ConfigurationManager.AppSettings["ExpiryWeek"]
            };
        }
        private void CheckToken()
        {
            if (!string.IsNullOrEmpty(tradeSetting.TokenCreatedOn) && !string.IsNullOrEmpty(tradeSetting.Token))
            {
                DateTime saveDate = DateTime.Parse(tradeSetting.TokenCreatedOn);
                if (DateTime.Now.Day != saveDate.Day)
                    chkTokenGenerated.IsChecked = false;
            }
            else
            {
                chkTokenGenerated.IsChecked = false;
            }
        }
        private void SetGmailSetting()
        {
            gmailConnect = new GmailConnect();
            gmailConnect.OnUniversalCondition += GmailConnect_OnUniversalCondition;
        }
        private async void GmailConnect_OnUniversalCondition(object sender, EventArgs e)
        {
            await apiProcessor.GetOrderHistory();
            await apiProcessor.CancelAllPendingOrder();
        }
          
        private async void btnEntry_Click(object sender, RoutedEventArgs e)
        {
            btnEntry.IsEnabled = false;
            
            if (txtStrike.Text == string.Empty)
            {   
                MessageBox.Show("Enter strike");
                btnEntry.IsEnabled = true;
                return;
            }
            apiProcessor.Strike = txtStrike.Text;
            await apiProcessor.PlaceEntryOrder();
            System.Threading.Thread.Sleep(2000);
            await apiProcessor.GetOrderHistory();
            await apiProcessor.PlaceStopLossOrder();
            btnEntry.IsEnabled = true;
        }

      
        private async void btnToken_Click(object sender, RoutedEventArgs e)
        {
            btnToken.IsEnabled = false;
            await apiProcessor.Login();
            chkTokenGenerated.IsChecked = true;
            btnToken.IsEnabled = true;
        }

        private async void btnExit_Click(object sender, RoutedEventArgs e)
        {
            btnExit.IsEnabled = false;
            await apiProcessor.GetOrderHistory(); 
            await apiProcessor.ExitAllOrders();
            
            btnExit.IsEnabled = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if(apiProcessor != null)
                apiProcessor.Dispose();
            if(gmailConnect != null)
                gmailConnect.Dispose();
        }

        private async void btnPEExit_Click(object sender, RoutedEventArgs e)
        { 
            btnPEExit.IsEnabled = false;
            await apiProcessor.GetOrderHistory();
            await apiProcessor.ExitPEOrders();
            btnPEExit.IsEnabled = true;
        }

        private async void btnCEExit_Click(object sender, RoutedEventArgs e)
        {
            btnCEExit.IsEnabled = false;
            await apiProcessor.GetOrderHistory();
            await apiProcessor.ExitCEOrders();
            btnCEExit.IsEnabled = true;
        }

        private void btnReadEmail_Click(object sender, RoutedEventArgs e)
        {
            if (btnReadEmail.Content.ToString() == "Read Email")
            {
                btnReadEmail.Content = "Stop Email";
                SetGmailSetting();
            }
            else
            {
                btnReadEmail.Content = "Read Email";
                gmailConnect.Dispose();
            }
            
        }

        private void btnMTMExit_Click(object sender, RoutedEventArgs e)
        {
            mtmConnect = new MTMConnect(apiProcessor);
            
        }
    }
}
