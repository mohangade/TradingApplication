using AliceBlueWrapper.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AliceBlueWrapper
{

    public class TickerTape : IDisposable
    {

        private IWebSocket _ws;
        public string Token;
        private readonly string base_url = "wss://ant.aliceblueonline.com/hydrasocket/v2/websocket?access_token={0}";
        bool IsConnected = false;
        public string bankNiftyValue = string.Empty;
        public delegate void OnTickHandler(Tick TickData);
        public event OnTickHandler OnTick;
        public TickerTape(string accessToken)
        {
            Token = accessToken;

            if (_ws == null)
                _ws = new WebSocket();

            _ws.OnConnect += _ws_OnConnect;
            _ws.OnData += _ws_OnData;
            _ws.OnClose += _ws_OnClose;
            _ws.OnError += _ws_OnError;

            string url = string.Format(base_url, Token);
            _ws.Connect(url);

        }

        private void _ws_OnError(string errorMessage)
        {

        }

        private void _ws_OnClose()
        {

        }

        private void _ws_OnData(byte[] data, int count, string messageType)
        {
            Tick tick = new Tick();
             short exhange = BitConverter.ToInt16(data, 1);

            int exchange1 = BitConverter.ToInt32(data, 3);
            int offset = 2;
            tick.InstrumentToken = ReadInt(data, ref offset);
            offset = 6;
           // UInt32 ltp = ReadInt(data, ref offset) / 100;
            tick.LastPrice = ReadInt(data, ref offset) / 100;
            offset = 66;
            tick.Open = ReadInt(data, ref offset) / 100;            
            offset = 74;
            tick.Close = ReadInt(data, ref offset) / 100;

            bankNiftyValue = Convert.ToString(tick.LastPrice);
            if (tick.InstrumentToken != 0 && IsConnected )
            {
                OnTick(tick);
            }
        }

        private void _ws_OnConnect()
        {
            IsConnected = true;       
        }


        /// <summary>
        /// Reads 4 byte int32 from byte stream
        /// </summary>
        private UInt32 ReadInt(byte[] b, ref int offset)
        {
            UInt32 data = (UInt32)BitConverter.ToUInt32(new byte[] { b[offset + 3], b[offset + 2], b[offset + 1], b[offset + 0] }, 0);
            offset += 4;
            return data;
        }

        /// <summary>
        /// Subscribe to a list of instrument_tokens.
        /// </summary>
        /// <param name="Tokens">List of instrument instrument_tokens to subscribe</param>
        public void Subscribe(int exchange, List<int> instrumentTokens)
        {
            if (exchange <= 0 && instrumentTokens.Count <= 0)
                return;
            List<string> stringTokens = new List<string>();
            foreach (var token in instrumentTokens)
            {
                stringTokens.Add("[" + exchange + "," + token + "]");
            }            
            string msg = "{\"a\": \"subscribe\", \"v\": [" + string.Join(",", stringTokens) + "], \"m\": \"marketdata\"}";//working silverm

            if (IsConnected)
                _ws.Send(msg);
        }

        public void Subscribe(int exchange, int instrumentToken)
        {
            if (exchange <= 0 && instrumentToken <= 0)
                return;

            string msg = "{\"a\": \"subscribe\", \"v\": [[" + exchange + "," + instrumentToken + "]], \"m\": \"marketdata\"}";//working silverm
            if (IsConnected)
                _ws.Send(msg);
        }
        public void UnSubscribe(int exchange, int instrumentToken)
        {
            if (exchange <= 0 && instrumentToken <= 0)
                return;

            string msg = "{\"a\": \"unsubscribe\", \"v\": [[" + exchange + "," + instrumentToken + "]], \"m\": \"marketdata\"}";//working silverm
            if (IsConnected)
                _ws.Send(msg);
        }
      
        public void UnSubscribe(int exchange, List<int> instrumentTokens)
        {
            if (exchange <= 0 && instrumentTokens.Count <= 0)
                return;

            List<string> stringTokens = new List<string>();
            foreach (var token in instrumentTokens)
            {
                stringTokens.Add("[" + exchange + "," + token + "]");
            }
            string msg = "{\"a\": \"unsubscribe\", \"v\": [" + string.Join(",", stringTokens) + "], \"m\": \"marketdata\"}";//working silverm

            if (IsConnected)
                _ws.Send(msg);
        }
        public void Dispose()
        {
            if (_ws != null && IsConnected)
            {
                _ws.Close();
                IsConnected = false;
                _ws = null;
            }
        }
    }
}
