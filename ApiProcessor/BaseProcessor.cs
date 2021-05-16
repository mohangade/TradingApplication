using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AliceBlueWrapper;
using AliceBlueWrapper.Models;
using ApiProcessor.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiProcessor
{
    public delegate void LogHandler(string logMessage);
    public class BaseProcessor : IDisposable
    {
        #region Private Variables
        private List<Order> pendingOrders;
        private List<Order> completedOrder;
        private AliceBlue aliceBlue;
        private TradeSetting tradeSetting;
        private List<Instrument> masterContact;
        private Helper helper;

        #endregion

        public bool IsStrangle = false;
        public bool IsCEOrder = false;
        public bool IsPEOrder = false;

        public int OtmDifference { get; set; }
        public int ExitOrderRetryCount = 3;
        public event LogHandler LogAdded;
        public int Strike { get; set; }
        public int CEStrike { get; set; }
        public int PEStrike { get; set; }
        public CashDetail Cash { get; set; }       
        public string UserTransType { get; set; }
        public List<OrderResponse> ExecutedOrders { get; set; }
        public BaseProcessor(TradeSetting tradeSetting, Helper helper)
        {
            aliceBlue = new AliceBlue();
            this.tradeSetting = tradeSetting;
            this.helper = helper;
            masterContact = new List<Instrument>();
            ExecutedOrders = new List<OrderResponse>();
        }

        public void LoadMasterContract()
        {
            try
            {
                string masterContractJson =
                System.IO.File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}\\Data\\contract.json");
                string path = "$.NSE-OPT";
                JToken jToken = JToken.Parse(masterContractJson).SelectToken(path);
                if(jToken!= null)
                    masterContact = JsonConvert.DeserializeObject<List<Instrument>>(jToken.ToString());
                else
                     LogAdded(path + " not found");
            }
            catch (Exception ex)
            {
                LogAdded("Error while loading master contract.");
                LogAdded(ex.Message);
            }
        }

        public async Task GetMasterContract()
        {
            try
            {
                string masterContractJson = string.Empty;

                masterContractJson = await aliceBlue.GetMasterContract();
                System.IO.File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}\\Data\\contract.json", masterContractJson);
                if (!string.IsNullOrEmpty(masterContractJson))
                {
                    LogAdded("Master contract downloaded successfully.");
                    LoadMasterContract();
                }
                else
                    LogAdded("Error Master contract download failed.");
            }
            catch (Exception ex)
            {
                LogAdded(ex.Message);
            }
        }

        public async Task Login()
        {
            try
            {
                LoginDetail loginDetail = new LoginDetail
                {
                    client_id = tradeSetting.ClientId,
                    client_secret = tradeSetting.ClientSecret,
                    userId = tradeSetting.UserId,
                    password = tradeSetting.Password,
                    answer1 = tradeSetting.Answer1,
                    answer2 = tradeSetting.Answer2
                };
                Token token = await aliceBlue.LoginAndGetToken(loginDetail);
                if (token.AccessToken != string.Empty)
                {
                    tradeSetting.Token = token.AccessToken;
                    helper.UpdateConfigKey("Token", token.AccessToken);
                    helper.UpdateConfigKey("TokenCreatedOn", DateTime.Now.ToString());
                    LogAdded("Token created successfully");
                }
                else
                    LogAdded("Access token is empty.");
            }

            catch (Exception ex)
            {
                LogAdded(ex.Message);
            }
        }

        public async Task PlaceEntryOrder()
        {
            try
            {
                OrderResponse response = null;
                if (Strike > 0)
                {
                    if (IsStrangle)
                    {
                        CEStrike = Strike + OtmDifference;
                        PEStrike = Strike - OtmDifference;
                    }
                    else
                    {
                        CEStrike = Strike;
                        PEStrike = Strike;
                    } 
                }
                else
                {
                    if (CEStrike > 0 && PEStrike > 0)
                        LogAdded("Placing order from strategy");
                    else
                        return;
                }
                

                //place CE order
                if (IsCEOrder)
                {
                    response = await aliceBlue.PlaceOrder(tradeSetting.Token,
                        new Order
                        {
                            order_type = OrderType.Market,
                            instrument_token = GetInstrumentTokenForFNO($"BANKNIFTY {tradeSetting.ExpiryWeek} {CEStrike.ToString()}.0 CE"),
                            quantity = tradeSetting.Quantity,
                            transaction_type = UserTransType,
                            product = ProductType.Intraday
                        });
                    ExecutedOrders.Add(response);
                    LogAdded($"BANKNIFTY{tradeSetting.ExpiryWeek}{CEStrike}CE  : {response.message} status: {response.status}");
                }
                //place PE order
                if (IsPEOrder)
                {
                    response = await aliceBlue.PlaceOrder(tradeSetting.Token,
                     new Order
                     {
                         order_type = OrderType.Market,
                         instrument_token = GetInstrumentTokenForFNO($"BANKNIFTY {tradeSetting.ExpiryWeek} {PEStrike.ToString()}.0 PE"),
                         quantity = tradeSetting.Quantity,
                         transaction_type = UserTransType,
                         product = ProductType.Intraday
                     });
                    ExecutedOrders.Add(response);
                    LogAdded($"BANKNIFTY{tradeSetting.ExpiryWeek}{PEStrike}PE  : {response.message} status: {response.status}");
                }
            }
            catch (Exception ex)
            {
                LogAdded("PlaceEntryOrder failed");
                LogAdded(ex.Message);
            }
        }

        public async Task PlaceStopLossOrder(List<OrderResponse> executedOrders)
        {
            try
            {
                OrderResponse response;

                foreach (OrderResponse orderResponse in executedOrders)
                {
                    Order order = completedOrder.SingleOrDefault(x => x.oms_order_id == orderResponse.data.oms_order_id
                                                                    && x.order_status == "complete");
                    if (order != null)
                    {
                        double.TryParse(order.average_price, out double orderPrice);

                        if (orderPrice > 0 && (order.order_type == OrderType.Market || order.order_type == OrderType.Limit))
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
                                    order_type = OrderType.StopLossLimit,
                                    instrument_token = order.instrument_token,
                                    quantity = order.quantity,
                                    transaction_type = transactionType,
                                    product = order.product,
                                    trigger_price = ((int)orderPrice).ToString(),
                                    price = ((int)orderPrice).ToString()
                                });
                        }
                        LogAdded($"StopLoss order for { order.trading_symbol} placed.");
                    }
                }
                executedOrders.Clear();
            }
            catch (Exception ex)
            {
                LogAdded("PlaceStopLossOrder failed");
                LogAdded(ex.Message);
            }
        }

        public async Task GetOrderHistory()
        {
            try
            {
                OrderHistory orderHistory = await aliceBlue.GetOrderHistory(tradeSetting.Token);
                pendingOrders = orderHistory.data.pending_orders;
                completedOrder = orderHistory.data.completed_orders;
            }
            catch (Exception ex)
            {
                LogAdded("GetOrderHistory Failed");
                LogAdded(ex.Message);
            }
        }

        public async Task<DayPosition> GetDayPosition()
        {
            try
            {
                return await aliceBlue.GetDayPosition(tradeSetting.Token);
            }
            catch (Exception ex)
            {
                LogAdded("GetDayPostion failed");
                throw ex;
            }
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
            try
            {
                await GetOrderHistory();
                foreach (var order in pendingOrders)
                {
                    await CancelOrder(order.oms_order_id);
                }
                DayPosition dayPosition = await aliceBlue.GetDayPosition(tradeSetting.Token);

                string transactionType = string.Empty;
                foreach (Position position in dayPosition.data.positions)
                {

                    if (position.net_quantity != 0)
                    {
                        if (position.net_quantity > 0)
                            transactionType = TransactionType.Sell;
                        else
                            transactionType = TransactionType.Buy;

                        Order squareoffOrder = new Order
                        {
                            order_type = OrderType.Market,
                            instrument_token = position.instrument_token,
                            transaction_type = transactionType,
                            quantity = Math.Abs(position.net_quantity).ToString(),
                            product = position.product,
                        };

                        await aliceBlue.PlaceOrder(tradeSetting.Token, squareoffOrder);
                    }
                }
                LogAdded("Exited all pending order.");
            }
            catch (Exception ex)
            {
                LogAdded("ExitAllOrders failed");
                if (ExitOrderRetryCount != 0)
                {
                    ExitOrderRetryCount--;
                    Thread.Sleep(500);
                    LogAdded("ExitAllOrders Retry");
                    await ExitAllOrders();
                }
                LogAdded(ex.Message);
            }
        }
        public async Task ExitCEOrders()
        {
            IEnumerable<Order> ceOrders = pendingOrders.Where(x => x.trading_symbol.ToLower().Contains("ce"));
            foreach (var order in ceOrders)
            {
                //await CancelAndPlaceMarketOrder(order);
            }
        }
        public async Task ExitPEOrders()
        {
            IEnumerable<Order> ceOrders = pendingOrders.Where(x => x.trading_symbol.ToLower().Contains("pe"));
            foreach (var order in ceOrders)
            {
                // await CancelAndPlaceMarketOrder(order);
            }
        }

        private async Task CancelOrder(string orderId)
        {
            try
            {
                await aliceBlue.CancelOrder(tradeSetting.Token, orderId);
            }
            catch (Exception ex)
            {
                LogAdded("CancelOrder failed");
                LogAdded(ex.Message);
            }
        }
        public string GetInstrumentTokenForFNO(string trading_symbol)
        {
            Instrument ins = masterContact.Single(x => x.symbol.Contains(trading_symbol));
            return ins.code;
        }
        public string Token
        {
            get
            {
                return tradeSetting.Token;
            }
        }
        public void Dispose()
        {
            aliceBlue.Dispose();
        }
    }
}
