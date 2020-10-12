using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Trading_App
{
    public class Helper 
    {       
        public void UpdateConfigKey(string strKey, string newValue)
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
    }
}
