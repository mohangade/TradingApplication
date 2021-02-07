using AliceBlueWrapper;
using AliceBlueWrapper.Models;
using ApiProcessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Trading_App.Common
{
    public delegate void MTMHandler(MTMDetail mTMDetail);
    public delegate void LogHandler(string message);
    public class MTMConnect : IDisposable
    {


        public event MTMHandler OnMTMChanged;
        public event LogHandler OnMTMTargetHit;
        public event OnErrorHandler OnError;

        Timer timer;
        BaseProcessor apiProcessor;
        bool timerStop = false;
        public double TargerMTM = 0;
        public double MaxLossMTM = -2000;
        double High { get; set; }
        double Low { get; set; }
        DayPosition dayPosition;
        public MTMConnect(BaseProcessor apiProcessor)
        {
            this.apiProcessor = apiProcessor;
            timer = new Timer
            {
                Interval = 1000
            };
        }
        public void TimerStart()
        {
            //double.TryParse(apiProcessor.TargetMTM, out targerMTM);
            //double.TryParse(apiProcessor.MaxMTMLoss, out maxLossMTM);
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
                    OnMTMChanged?.Invoke(new MTMDetail { MTM = mtmVal.ToString(), High = High.ToString(),Low = Low.ToString()});
                    SetHighLowMTM(mtmVal);
                    if ((mtmVal >= TargerMTM || mtmVal <= MaxLossMTM) && !timerStop)
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

        private void SetHighLowMTM(double mtmVal)
        {
            if(mtmVal>0)
            {
                if (High < mtmVal)
                    High = mtmVal;
            }
            else
            {
                if (Low > mtmVal)
                    Low = mtmVal;
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

    public struct MTMDetail
    {
        public string MTM { get; set; }
        public string High { get; set; }
        public string Low { get; set; }
    }
}