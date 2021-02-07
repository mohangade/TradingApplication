using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;

namespace AliceBlueWrapper
{
    public class Utils
    {
        /// <summary>
        /// Convert string to Date object
        /// </summary>
        /// <param name="obj">Date string.</param>
        /// <returns>Date object/</returns>
        public static DateTime? StringToDate(string DateString)
        {
            if (String.IsNullOrEmpty(DateString))
                return null;

            try
            {
                if (DateString.Length == 10)
                {
                    return DateTime.ParseExact(DateString, "yyyy-MM-dd", null);
                }
                else
                {
                    return DateTime.ParseExact(DateString, "yyyy-MM-dd HH:mm:ss", null);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Converts string to decimal. Handles culture and scientific notations.
        /// </summary>
        /// <param name="value">Input string.</param>
        /// <returns>Decimal value</returns>
        public static decimal StringToDecimal(String value)
        {
            return decimal.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parse instruments API's CSV response.
        /// </summary>
        /// <param name="Data">Response of instruments API.</param>
        /// <returns>CSV data as array of nested string dictionary.</returns>
        //public static List<Dictionary<string, dynamic>> ParseCSV(string Data)
        //{
        //    string[] lines = Data.Split('\n');

        //    List<Dictionary<string, dynamic>> instruments = new List<Dictionary<string, dynamic>>();

        //    using (var parser = new CsvTextFieldParser(StreamFromString(Data)))
        //    {
        //        // parser.CommentTokens = new string[] { "#" };
        //        // parser.SetDelimiters(new string[] { "," });
        //        parser.HasFieldsEnclosedInQuotes = true;

        //        // Skip over header line.
        //        string[] headers = parser.ReadFields();

        //        while (!parser.EndOfData)
        //        {
        //            string[] fields = parser.ReadFields();
        //            Dictionary<string, dynamic> item = new Dictionary<string, dynamic>();

        //            for (var i = 0; i < headers.Length; i++)
        //                item.Add(headers[i], fields[i]);

        //            instruments.Add(item);
        //        }
        //    }

        //    return instruments;
        //}

        /// <summary>
        /// Wraps a string inside a stream
        /// </summary>
        /// <param name="value">string data</param>
        /// <returns>Stream that reads input string</returns>
        public static MemoryStream StreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
        }

        /// <summary>
        /// Helper function to add parameter to the request only if it is not null or empty
        /// </summary>
        /// <param name="Params">Dictionary to add the key-value pair</param>
        /// <param name="Key">Key of the parameter</param>
        /// <param name="Value">Value of the parameter</param>
        public static void AddIfNotNull(Dictionary<string, dynamic> Params, string Key, string Value)
        {
            if (!String.IsNullOrEmpty(Value))
                Params.Add(Key, Value);
        }

        /// <summary>
        /// Generates SHA256 checksum for login.
        /// </summary>
        /// <param name="Data">Input data to generate checksum for.</param>
        /// <returns>SHA256 checksum in hex format.</returns>
        public static string SHA256(string Data)
        {
            SHA256Managed sha256 = new SHA256Managed();
            StringBuilder hexhash = new StringBuilder();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(Data), 0, Encoding.UTF8.GetByteCount(Data));
            foreach (byte b in hash)
            {
                hexhash.Append(b.ToString("x2"));
            }
            return hexhash.ToString();
        }

        /// <summary>
        /// Creates key=value with url encoded value
        /// </summary>
        /// <param name="Key">Key</param>
        /// <param name="Value">Value</param>
        /// <returns>Combined string</returns>
        //public static string BuildParam(string Key, dynamic Value)
        //{
        //    if (Value is string)
        //    {
        //        return HttpUtility.UrlEncode(Key) + "=" + HttpUtility.UrlEncode((string)Value);
        //    }
        //    else
        //    {
        //        string[] values = (string[])Value;
        //        return String.Join("&", values.Select(x => HttpUtility.UrlEncode(Key) + "=" + HttpUtility.UrlEncode(x)));
        //    }
        //}

        /// <summary>
        /// Converts Unix timestamp to DateTime
        /// </summary>
        /// <param name="unixTimeStamp">Unix timestamp in seconds.</param>
        /// <returns>DateTime object.</returns>
        public static DateTime UnixToDateTime(UInt64 unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 5, 30, 0, 0, DateTimeKind.Unspecified);
            dateTime = dateTime.AddSeconds(unixTimeStamp);
            return dateTime;
        }
    }
}
