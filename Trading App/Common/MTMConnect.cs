using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Trading_App.Processor;

namespace Trading_App.Common
{
    public class MTMConnect
    {
        Timer timer = new Timer();
        APIProcessor apiProcessor;
        public MTMConnect(APIProcessor apiProcessor)
        {
            this.apiProcessor = apiProcessor;
            timer.Interval = 2000;
            timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
            timer.Start();
        }


        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await apiProcessor.GetCashDetails();
            if(apiProcessor.Cash != null)
            {
                string mtm = apiProcessor.Cash.data.cash_positions.Single(x => x.segment == "ALL").utilized.unrealised_m2m;
                double.TryParse(mtm, out double result);

                if(result > 250)
                {
                    await apiProcessor.ExitAllOrders();
                    this.timer.Stop();
                    timer.Dispose();
                }
            }
          
        }
    }
}
