using AliceBlueWrapper;
using AliceBlueWrapper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Trading_App.Processor;

namespace Trading_App.Common
{
    public delegate void MTMHandler(string value);
    public class MTMConnect : IDisposable
    {


        public event MTMHandler OnMTMChanged;
        public event MTMHandler OnMTMTargetHit;
        public event OnErrorHandler OnError;

        Timer timer;
        APIProcessor apiProcessor;
        bool timerStop = false;
        double targerMTM = 0;
        double maxLossMTM = 2000;
        DayPosition dayPosition;
        public MTMConnect(APIProcessor apiProcessor)
        {
            this.apiProcessor = apiProcessor;
            timer = new Timer
            {
                Interval = 1000
            };
        }
        public void TimerStart()
        {
            double.TryParse(apiProcessor.TargetMTM, out targerMTM);
            double.TryParse(apiProcessor.MaxMTMLoss, out maxLossMTM);
            timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
            timer.Start();
            timerStop = false;            
        }
        public void TimerStop()
        {
            timer.Elapsed -= new ElapsedEventHandler(Timer_Elapsed);
            timer.Stop();
            timerStop = true;
        }
        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                dayPosition = await apiProcessor.GetDayPosition();

                if (dayPosition.data.positions.Any())
                {
                    double mtmVal = GetMTMValue(dayPosition);
                    OnMTMChanged?.Invoke((mtmVal.ToString()));
                    if ((mtmVal >= targerMTM || mtmVal <= maxLossMTM )&& !timerStop)
                    {
                        TimerStop();
                        apiProcessor.ExitOrderRetryCount = 3;
                        await apiProcessor.ExitAllOrders();
                        OnMTMTargetHit("Target hit");
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex.Message);
            }
        }

        public double GetMTMValue(DayPosition dayPosition)
        {
            double mtmval = 0;
            foreach (Position position in dayPosition.data.positions)
            {
                mtmval += double.Parse(position.m2m);
            }
            return mtmval;
        }

        public async Task<double> GetFinalMTM()
        {
            dayPosition = await apiProcessor.GetDayPosition();
            return GetMTMValue(dayPosition);
        }
        public void Dispose()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
            }
            OnMTMChanged = null;
        }
    }
}
