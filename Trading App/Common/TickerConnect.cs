using AliceBlueWrapper;
using AliceBlueWrapper.Models;
using ApiProcessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Trading_App.Common
{
    public delegate void TickerHandler(List<SubscribeToken> list);
    public class TickerConnect : IDisposable
    {
        public event TickerHandler OnTickerChanged;
        public event OnErrorHandler OnError;
        public List<SubscribeToken> subscribeTokens = new List<SubscribeToken>();
        Timer timer;
        BaseProcessor apiProcessor;
        TickerTape tickerTape = null;

        public TickerConnect(BaseProcessor apiProcessor)
        {
            this.apiProcessor = apiProcessor;
           

            timer = new Timer
            {
                Interval = 500
            };

            AddTokens();
        }

        private void AddTokens()
        {
            subscribeTokens.Add(new SubscribeToken
            {
                InstrumentToken = 219001,
                Symbol = "SILVER MAR FUT"
            });
            subscribeTokens.Add(new SubscribeToken
            {
                InstrumentToken = 224233,
                Symbol = "CRUDEOIL FEB FUT"
            });
        }

        public void TimerStart()
        {
            
            timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {

               // string tickerValue = tickerTape.bankNiftyValue;
                
                if (subscribeTokens != null && OnTickerChanged != null)
                {
                    OnTickerChanged?.Invoke(subscribeTokens);
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex.Message);
            }
        }

        public void SubscribeTicker(string token)
        {
            tickerTape = new TickerTape(token);
            tickerTape.Subscribe(4, 219001);
            tickerTape.Subscribe(4, 224233);
            tickerTape.OnTick += TickerTape_OnTick;

            TimerStart();
            
        }

        private void TickerTape_OnTick(AliceBlueWrapper.Models.Tick TickData)
        {
            try
            {
                foreach (var token in subscribeTokens.Where(x => x.InstrumentToken == TickData.InstrumentToken))
                {
                    token.Tick = TickData;
                }
               
                
                //string tickerValue = TickData.LastPrice.ToString();

                //if (!string.IsNullOrEmpty(tickerValue))
                //{
                //    OnTickerChanged?.Invoke(tickerValue);
                //}
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex.Message);
            }
        }

        public void Dispose()
        {
            if (tickerTape != null)
                tickerTape.Dispose();
        }
    }
}
