using System;
using System.Collections.Generic;
using System.Text;

namespace Trading_App.Model
{
    public class TradeSetting
    {
        public string UserId { get; set; }

        public string Password { get; set; }

        public string ClientSecret { get; set; }

        public string Token { get; set; }

        public string TokenCreatedOn { get; set; }

        public string Quantity { get; set; }

        public string StopLossPercentage { get; set; }

        public string ExpiryWeek { get; set; }

    }
}
