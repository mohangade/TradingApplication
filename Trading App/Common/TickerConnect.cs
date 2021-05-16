//using AliceBlueWrapper;
//using AliceBlueWrapper.Models;
//using ApiProcessor;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Timers;

//namespace Trading_App.Common
//{
//    public delegate void TickerHandler(List<SubscribeToken> list);
//    public class TickerConnect : IDisposable
//    {
//        public event TickerHandler OnTickerChanged;
//        public event OnErrorHandler OnError;
//        public List<SubscribeToken> SubscribeTokenList = new List<SubscribeToken>();
//        Timer timer;
//        BaseProcessor apiProcessor;
//        TickerTape tickerTape ;

//        public TickerConnect(BaseProcessor apiProcessor)
//        {
//            this.apiProcessor = apiProcessor;
//            tickerTape = new TickerTape(apiProcessor.Token);
//            tickerTape.OnTick += TickerTape_OnTick;

//            timer = new Timer{ Interval = 500 };            
//            timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
//            timer.Start();            
//        }

//        //private void AddTokens()
//        //{
//        //    subscribeTokens.Add(new SubscribeToken
//        //    {
//        //        InstrumentToken = 219001,
//        //        Symbol = "SILVER MAR FUT"
//        //    });
//        //    subscribeTokens.Add(new SubscribeToken
//        //    {
//        //        InstrumentToken = 224233,
//        //        Symbol = "CRUDEOIL FEB FUT"
//        //    });
//        //}
        
//        public void SubscribeToken(Instrument token)
//        {
//            tickerTape.Token = apiProcessor.Token;
//            int code = int.Parse(token.code);
//            subscribeTokens.Add(new SubscribeToken
//            { 
//                InstrumentToken = code,
//                Symbol = token.symbol                
//            });
//            tickerTape.Subscribe(int.Parse( token.exchange), code);
//        }
//        public void UnSubscribeToken(Instrument token)
//        {
//            int code = int.Parse(token.code);
//            subscribeTokens.RemoveAll(x => x.InstrumentToken == code);
//            tickerTape.UnSubscribe(int.Parse(token.exchange), code);
//        }
//        private void TickerTape_OnTick(AliceBlueWrapper.Models.Tick TickData)
//        {
//            try
//            {
//                foreach (var token in subscribeTokens.Where(x => x.InstrumentToken == TickData.InstrumentToken))
//                {
//                    token.Tick = TickData;
//                }
//            }
//            catch (Exception ex)
//            {
//                OnError?.Invoke(ex.Message);
//            }
//        }

//        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
//        {
//            try
//            {
//                if (subscribeTokens != null && OnTickerChanged != null)
//                {
//                    OnTickerChanged?.Invoke(subscribeTokens);
//                }
//            }
//            catch (Exception ex)
//            {
//                OnError?.Invoke(ex.Message);
//            }
//        }
//        public void Dispose()
//        {
//            if (tickerTape != null)
//                tickerTape.Dispose();
//        }
//    }
//}
