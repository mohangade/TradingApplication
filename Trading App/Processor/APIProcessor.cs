using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AliceBlueWrapper;
using AliceBlueWrapper.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Trading_App.Model;

namespace Trading_App.Processor
{
    public class APIProcessor : IDisposable
    {
        #region Private Variables
        private List<Order> pendingOrders;
        private List<Order> completedOrder;
        private AliceBlue aliceBlue;
        private TradeSetting tradeSetting;
        private List<Instrument> masterContact;
        private Helper helper;
        #endregion

        public string Strike { get; set; }
        public CashDetail Cash { get; set; }
        public APIProcessor(TradeSetting tradeSetting)
        {
            aliceBlue = new AliceBlue();
            this.tradeSetting = tradeSetting;
            masterContact = new List<Instrument>();
            helper = new Helper();
        }
        public void LoadMasterContract()
        {
            string masterContractJson =
                System.IO.File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}\\Data\\contract.json");
            JToken jToken = JToken.Parse(masterContractJson).SelectToken("$.NSE-OPT");
            masterContact = JsonConvert.DeserializeObject<List<Instrument>>(jToken.ToString());
        }
        public async Task Login()
        {
            Token token = await aliceBlue.LoginAndGetToken(tradeSetting.UserId, tradeSetting.ClientSecret, tradeSetting.Password);

            tradeSetting.Token = token.AccessToken;
            helper.UpdateConfigKey("Token", token.AccessToken);
            helper.UpdateConfigKey("TokenCreatedOn", DateTime.Now.ToString());
        }
        public async Task PlaceEntryOrder()
        {
            //place CE order
            string res = await aliceBlue.PlaceOrder(tradeSetting.Token,
                new Order
                {
                    order_type = OrderType.Market,
                    instrument_token = GetInstrumentTokenForFNO($"BANKNIFTY {tradeSetting.ExpiryWeek} {Strike} CE"),
                    quantity = tradeSetting.Quantity,
                    transaction_type = TransactionType.Sell,
                    product = ProductType.Intraday
                });

            //place PE order
            res = await aliceBlue.PlaceOrder(tradeSetting.Token,
                 new Order
                 {
                     order_type = OrderType.Market,
                     instrument_token = GetInstrumentTokenForFNO($"BANKNIFTY {tradeSetting.ExpiryWeek} {Strike} PE"),
                     quantity = tradeSetting.Quantity,
                     transaction_type = TransactionType.Sell,
                     product = ProductType.Intraday
                 });
        }
        public async Task PlaceStopLossOrder()
        {
            string response = string.Empty;
            foreach (var order in completedOrder)
            {
                if (order.order_status.ToLower() == "completed")
                {
                    double.TryParse(order.price, out double orderPrice);

                    if (orderPrice > 0 && order.order_type == OrderType.Market)
                    {
                        string transactionType;
                        double.TryParse(tradeSetting.StopLossPercentage, out double stopLoss);
                        if (order.transaction_type == TransactionType.Buy)
                        {
                            transactionType = TransactionType.Sell;
                            orderPrice -= (orderPrice * (stopLoss / 100));
                        }
                        else
                        {
                            transactionType = TransactionType.Buy;
                            orderPrice += (orderPrice * (stopLoss / 100));
                        }
                        response = await aliceBlue.PlaceOrder(tradeSetting.Token,
                            new Order
                            {
                                order_type = OrderType.StopLossMarket,
                                instrument_token = order.instrument_token,
                                quantity = order.quantity,
                                transaction_type = transactionType,
                                product = order.product,
                                trigger_price = orderPrice.ToString()
                            });

                    }
                }
            }
        }
        public async Task GetOrderHistory()
        {
            OrderHistory orderHistory = await aliceBlue.GetOrderHistory(tradeSetting.Token);
            pendingOrders = orderHistory.data.pending_orders;
            completedOrder = orderHistory.data.completed_orders;
        }
        public async Task GetCashDetails()
        {
            Cash = await aliceBlue.GetCash(tradeSetting.Token);
        }
        public async Task CancelAllPendingOrder()
        {
            foreach (var order in pendingOrders)
            {
                await CancelOrder(order.oms_order_id);
            }
        }
        public async Task ExitAllOrders()
        {
            foreach (var order in pendingOrders)
            {
                await CancelAndPlaceMarketOrder(order);
            }
        }
        public async Task ExitCEOrders()
        {
            IEnumerable<Order> ceOrders = pendingOrders.Where(x => x.trading_symbol.ToLower().Contains("ce"));
            foreach (var order in ceOrders)
            {
                await CancelAndPlaceMarketOrder(order);
            }
        }
        public async Task ExitPEOrders()
        {
            IEnumerable<Order> ceOrders = pendingOrders.Where(x => x.trading_symbol.ToLower().Contains("pe"));
            foreach (var order in ceOrders)
            {
                await CancelAndPlaceMarketOrder(order);
            }
        }
        private async Task CancelAndPlaceMarketOrder(Order order)
        {
            await aliceBlue.CancelOrder(tradeSetting.Token, order.oms_order_id);
            order.order_type = OrderType.Market;
            order.oms_order_id = string.Empty;
            order.price = "0.0";
            await aliceBlue.PlaceOrder(tradeSetting.Token, order);
        }
        private async Task CancelOrder(string orderId)
        {
            string response = await aliceBlue.CancelOrder(tradeSetting.Token, orderId);
        }
        private string GetInstrumentTokenForFNO(string symbol)
        {
            Instrument ins = masterContact.Single(x => x.symbol.Contains(symbol));
            return ins.code;
        }
        public void Dispose()
        {
            aliceBlue.Dispose();
        }
    }
}
