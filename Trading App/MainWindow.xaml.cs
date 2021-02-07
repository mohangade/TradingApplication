using AliceBlueWrapper;
using AliceBlueWrapper.Models;
using ApiProcessor;
using ApiProcessor.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Trading_App.Common;

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
        private TickerConnect tickerConnect;
        BaseProcessor apiProcessor;
        Helper helper;
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            helper = new Helper();
            tradeSetting = helper.ReadSetting();
            //SetGmailSetting();
            apiProcessor = new BaseProcessor(tradeSetting, helper);
            apiProcessor.LoadMasterContract();
            apiProcessor.LogAdded += LogAdded;

            mtmConnect = new MTMConnect(apiProcessor);
            mtmConnect.OnMTMChanged += UpdateMTM;
            mtmConnect.OnMTMTargetHit += MTMTargetHit;
            mtmConnect.OnError += MTMConnectError;

            tickerConnect = new TickerConnect(apiProcessor);
            tickerConnect.OnTickerChanged += TickerConnect_OnTickerChanged;
            tickerConnect.OnError += MTMConnectError;

            CheckToken();
            InitializeSetting();
            SubscribeTicker();
        }
        private void TickerConnect_OnTickerChanged(List<SubscribeToken> subscribeTokens)
        {
            Dispatcher.BeginInvoke(new TickerHandler(SetTiker), new object[] { subscribeTokens });
        }
        private void SetTiker(List<SubscribeToken> subscribeTokens )
        {
            string val = string.Empty;
            foreach (var item in subscribeTokens)
            {
                //val += " " + item.Tick.LastPrice;
                val += Environment.NewLine + item.Symbol + " : " + item.Tick.LastPrice;
            }
            txtLogs.Text = val;
            //tblMTM.Text = val;
            //strategyConnect.Strike = value;
        }
        public void SubscribeTicker()
        {
            if (chkTokenGenerated.IsChecked == true)
            {
                tickerConnect.SubscribeTicker(tradeSetting.Token);
            }
        }
        private void InitializeSetting()
        {
            txtMaxProfit.Text = tradeSetting.MTMProfit;
            txtMaxLoss.Text = tradeSetting.MTMLoss;
            tblExpiryWeek.Text = tradeSetting.ExpiryWeek;
            tblSL.Text = tradeSetting.StopLossPercentage;
            txtOTMDiff.Text = tradeSetting.OTMDiff;
        }

        private void LogAdded(string logMessage)
        {
            if (!Thread.CurrentThread.IsBackground)
                AddLogs(logMessage);
            else
                Dispatcher.BeginInvoke(new OnLogHandler(AddLogs), new object[] { logMessage });
        }
        private void AddLogs(string log)
        {
            txtLogs.Text += DateTime.Now.ToString() + ":  " + log + Environment.NewLine;
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

        #region gmail setting
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
        #endregion
        private async void btnEntry_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnEntry.IsEnabled = false;

                if (txtStrike.Text == string.Empty)
                {
                    MessageBox.Show("Enter strike");
                    btnEntry.IsEnabled = true;
                    return;
                }

                if (chkCallChecked.IsChecked == false && chkPutChecked.IsChecked == false)
                {
                    MessageBox.Show("Please select CE / PE option.");
                    btnEntry.IsEnabled = true;
                    return;
                }

                apiProcessor.IsCEChecked = Convert.ToBoolean(chkCallChecked.IsChecked);
                apiProcessor.IsPEChecked = Convert.ToBoolean(chkPutChecked.IsChecked);
                apiProcessor.UserTransType = comboBoxBUYSELL.Text;
                apiProcessor.IsStrangleChecked = Convert.ToBoolean(chkStrangleChecked.IsChecked);
                apiProcessor.Strike = Convert.ToInt32(txtStrike.Text);

                if (apiProcessor.IsStrangleChecked)
                {
                    if (!string.IsNullOrEmpty(txtOTMDiff.Text))
                        apiProcessor.OTMDiff = Convert.ToInt32(txtOTMDiff.Text);
                    else
                        apiProcessor.OTMDiff = 0;
                }

                await apiProcessor.PlaceEntryOrder();
                System.Threading.Thread.Sleep(10000);
                await apiProcessor.GetOrderHistory();
                //await apiProcessor.PlaceStopLossOrder(apiProcessor.ExecutedOrders);
                btnEntry.IsEnabled = true;
            }
            catch (Exception ex)
            {
                LogAdded(ex.Message);
            }
        }

        private async void btnToken_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnToken.IsEnabled = false;
                await apiProcessor.Login();
                chkTokenGenerated.IsChecked = true;
                btnToken.IsEnabled = true;               
            }
            catch (Exception ex)
            {
                AddLogs(ex.Message);
            }
        }

        private async void btnExit_Click(object sender, RoutedEventArgs e)
        {
            btnExit.IsEnabled = false;
            apiProcessor.ExitOrderRetryCount = 3;
            await apiProcessor.ExitAllOrders();

            btnExit.IsEnabled = true;
        }

        private void btnMTMExit_Click(object sender, RoutedEventArgs e)
        {
            // await apiProcessor.GetDayPosition();
            if (btnMTMExit.Content.ToString().Contains("Start"))
                StartMTM();
            else
                StopMTM();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            if (apiProcessor != null)
                apiProcessor.Dispose();
            if (gmailConnect != null)
                gmailConnect.Dispose();
        }

        #region Commented Code
        //private async void btnPEExit_Click(object sender, RoutedEventArgs e)
        //{
        //    btnPEExit.IsEnabled = false;
        //    await apiProcessor.GetOrderHistory();
        //    await apiProcessor.ExitPEOrders();
        //    btnPEExit.IsEnabled = true;
        //}

        //private async void btnCEExit_Click(object sender, RoutedEventArgs e)
        //{
        //    btnCEExit.IsEnabled = false;
        //    await apiProcessor.GetOrderHistory();
        //    await apiProcessor.ExitCEOrders();
        //    btnCEExit.IsEnabled = true;
        //}

        //private void btnReadEmail_Click(object sender, RoutedEventArgs e)
        //{
        //    if (btnReadEmail.Content.ToString() == "Read Email")
        //    {
        //        btnReadEmail.Content = "Stop Email";
        //        SetGmailSetting();
        //    }
        //    else
        //    {
        //        btnReadEmail.Content = "Read Email";
        //        gmailConnect.Dispose();
        //    }
        //} 
        #endregion      

        #region MTM 
        private void StartMTM()
        {
            if (txtMaxProfit.Text.Trim() == string.Empty)
            {
                MessageBox.Show("Please enter target MTM value.");
                return;
            }

            double.TryParse(txtMaxProfit.Text, out mtmConnect.TargerMTM);
            double.TryParse(txtMaxLoss.Text, out mtmConnect.MaxLossMTM);

            mtmConnect.TimerStart();
            btnMTMExit.Content = "Stop MTM";
            AddLogs("MTM timer started.");
        }
        private void StopMTM()
        {
            btnMTMExit.Content = "Start MTM";
            tblMTM.Text = "MTM value";
            mtmConnect.TimerStop();
            AddLogs("MTM timer ended.");
        }
        private void MTMTargetHit(string message)
        {
            try
            {
                LogAdded(message);
                string finalMTM = mtmConnect.GetFinalMTM().ToString();
                UpdateMTM(new MTMDetail { MTM = finalMTM});
                Dispatcher.BeginInvoke(new Action(StopMTM), null);
            }
            catch (Exception ex)
            {
                LogAdded(ex.Message);
            }
        }
        private void SetMTM(MTMDetail mtmVal)
        {
            tblMTM.Text = mtmVal.MTM;
        }
        private void UpdateMTM(MTMDetail mtmVal)
        {
            Dispatcher.BeginInvoke(new MTMHandler(SetMTM), new object[] { mtmVal });
        }
        private void MTMConnectError(string errorMessage)
        {
            LogAdded(errorMessage);
        }
        #endregion

        private void txtLogs_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            return;
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                tradeSetting.MTMProfit = txtMaxProfit.Text;
                tradeSetting.MTMLoss = txtMaxLoss.Text;

                mtmConnect.TargerMTM = double.Parse(txtMaxProfit.Text);
                mtmConnect.MaxLossMTM = double.Parse(txtMaxLoss.Text);

                AddLogs("MTM values updated.");
            }
            catch (Exception ex)
            {
                AddLogs("Error while updating MTM values.");
            }
        }

        private async void btnMasterContract_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                await apiProcessor.GetMasterContract();

            }
            catch (Exception ex)
            {
                AddLogs("Error while downloading master contract.");
            }
        }
    }
}