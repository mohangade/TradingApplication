using ApiProcessor;
using ApiProcessor.Common;
using ApiProcessor.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace AlgoEngine
{
    public class BaseEngine
    {
        internal TradeSetting tradeSetting;
        internal TickerHelper tickerHelper;
        internal BaseProcessor apiProcessor;
        internal Helper helper;
        public BaseEngine()
        {
            helper = new Helper();
            tradeSetting = helper.ReadSetting();

            apiProcessor = new BaseProcessor(tradeSetting, helper);
            apiProcessor.LoadMasterContract();
            tickerHelper = new TickerHelper(apiProcessor);
        }
    }
}
