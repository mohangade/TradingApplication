using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Xml;
using ApiProcessor.Model;

namespace ApiProcessor
{
    public class Helper 
    {       
        public void UpdateConfigKey(string strKey, string newValue)
        {

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                string configPath = AppDomain.CurrentDomain.BaseDirectory + "Trading App.dll.config";
                xmlDoc.Load(configPath);

                XmlNode appSettingsNode = xmlDoc.SelectSingleNode("configuration/appSettings");
                foreach (XmlNode childNode in appSettingsNode)
                {
                    if (childNode.Attributes["key"].Value == strKey)
                        childNode.Attributes["value"].Value = newValue;
                }
                xmlDoc.Save(configPath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public TradeSetting ReadSetting()
        {
            TradeSetting tradeSetting = new TradeSetting
            {
                UserId = ConfigurationManager.AppSettings["UserId"],
                Password = ConfigurationManager.AppSettings["Password"],
                ClientId = ConfigurationManager.AppSettings["ClientId"],
                ClientSecret = ConfigurationManager.AppSettings["ClientSecret"],
                Token = ConfigurationManager.AppSettings["Token"],
                TokenCreatedOn = ConfigurationManager.AppSettings["TokenCreatedOn"],
                Quantity = ConfigurationManager.AppSettings["Quantity"],
                StopLossPercentage = ConfigurationManager.AppSettings["StopLossPercentage"],
                ExpiryWeek = ConfigurationManager.AppSettings["ExpiryWeek"],
                Answer1 = ConfigurationManager.AppSettings["Answer1"],
                Answer2 = ConfigurationManager.AppSettings["Answer2"],
                MTMProfit = ConfigurationManager.AppSettings["MTMProfit"],
                MTMLoss = ConfigurationManager.AppSettings["MTMLoss"]
            };
            return tradeSetting;
        }
    }
}
