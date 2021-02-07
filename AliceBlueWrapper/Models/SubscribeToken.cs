using System;
using System.Collections.Generic;
using System.Text;

namespace AliceBlueWrapper.Models
{
    public class SubscribeToken
    {
        public int InstrumentToken { get; set; }

        public Tick Tick { get; set; }

        public string Symbol { get; set; }
    }
}
