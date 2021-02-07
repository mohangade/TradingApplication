using System;
using System.Collections.Generic;
using System.Text;

namespace AlgoEngine
{
    public class DumbStraddle
    {
        public OptionStrike  GetOptionStrike(int bankNiftyVal)
        {
            OptionStrike strike = new OptionStrike();
            int modVal = bankNiftyVal % 100;

            if(modVal <= 35)
            {
                strike.CE = strike.PE= bankNiftyVal - modVal;               
            }
            else if(modVal >35 && modVal< 70)
            {
                strike.CE = bankNiftyVal + (100 - modVal);
                strike.PE = bankNiftyVal - modVal;
            }
            else
            {
                strike.CE =strike.PE=  bankNiftyVal + (100 - modVal);
            }
            return strike;
        }


        public void RunStrategy()
        {
            //Subscribe bankNifty and get value

            int bankNiftyVal = 32542;
            OptionStrike strike = GetOptionStrike(bankNiftyVal);

            //subscribe CE
            //subscribe PE

            //Check value

            //if condition satishfy then Place Order
        }

    }
}
