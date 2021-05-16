using AliceBlueWrapper.Models;
using ApiProcessor;
using ApiProcessor.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AlgoEngine
{
    public class DumbStraddle : BaseEngine
    {
        public decimal? CurrentBankNifty { get; set; }
        public OptionStrike GetOptionStrike(int bankNiftyVal)
        {
            OptionStrike strike = new OptionStrike();
            int modVal = bankNiftyVal % 100;

            if (modVal <= 35)
            {
                strike.CE = strike.PE = bankNiftyVal - modVal;
            }
            else if (modVal > 35 && modVal < 70)
            {
                strike.CE = bankNiftyVal + (100 - modVal);
                strike.PE = bankNiftyVal - modVal;
            }
            else
            {
                strike.CE = strike.PE = bankNiftyVal + (100 - modVal);
            }
            return strike;
        }
        public DumbStraddle()
        {
            SubscribeBankNifty();
            tickerHelper.OnTickerChanged += TickerHelper_OnTickerChanged;
        }
        string bankNiftySymbol = "Nifty Bank";
        private void TickerHelper_OnTickerChanged(List<SubscribeToken> list)
        {
            CurrentBankNifty = list.Where(x => x.Symbol == bankNiftySymbol)?.First().Tick.LastPrice;
        }

        private void SubscribeBankNifty()
        {
            //"trading_symbol":"Nifty Bank",
            //"tradable":false,
            //"symbol":"Nifty Bank",
            //"index":true,
            //"exchange_code":1,
            //"exchange":"NSE",
            //"company":"Nifty Bank",
            //"code":"26009"
            Instrument bankNiftyInst = new Instrument
            {
                code = "26009",
                symbol = "Nifty Bank",
                exchange = "1"
            };
            tickerHelper.SubscribeToken(bankNiftyInst);
        }
        Timer timer;
        public void RunStrategy()
        {
            //Subscribe bankNifty and get value

            // TaskScheduler.Instance.ScheduleTask
            //subscribe CE
            //subscribe PE

            //Check value

            //if condition satishfy then Place Order
            TimerCallback timerCallback = new TimerCallback(PlaceOrderForthisStategy);

            DateTime firstRun = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 9,30, 0);
            TimeSpan timeSpan = firstRun - DateTime.Now;
            timeSpan = GetDueTime();
            if(timeSpan.TotalSeconds < 0)
                
            timer = new Timer(timerCallback,null, timeSpan, TimeSpan.FromMinutes(1));

           
        }

        private TimeSpan GetDueTime()
        {
            DateTime firstRun = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            switch (DateTime.Now.Hour)
            {
                case 9:
                    firstRun.AddHours(9);
                    break;
                case 10:
                    firstRun.AddHours(10);
                    break;
                case 11:
                    firstRun.AddHours(11);
                    break;
                default:
                    break;
            }
            if(DateTime.Now.Minute > 0 && DateTime.Now.Minute <=30) 
                firstRun.AddMinutes(30);
            else if (DateTime.Now.Minute > 30 && DateTime.Now.Minute <= 59)
                firstRun.AddHours(1);

            TimeSpan timeSpan = firstRun - DateTime.Now;
            return timeSpan;
        }

        private async void PlaceOrderForthisStategy(object obj)
        {
            if (DateTime.Now.Hour >= 11 && DateTime.Now.Minute > 1)
            {
                timer.Dispose();
                return;
            }
            OptionStrike optionStrike = new OptionStrike();
            if (CurrentBankNifty != null)
            {
                optionStrike = GetOptionStrike((int)CurrentBankNifty);
            }
            apiProcessor.IsCEOrder = true;
            apiProcessor.IsPEOrder = true;
            apiProcessor.CEStrike = optionStrike.CE;
            apiProcessor.PEStrike = optionStrike.PE;
            apiProcessor.LogAdded += ApiProcessor_LogAdded;
            apiProcessor.UserTransType = "SELL";
            await apiProcessor.PlaceEntryOrder();
        }
        private void ApiProcessor_LogAdded(string logMessage)
        {
            //throw new NotImplementedException();
        }
    }
}
