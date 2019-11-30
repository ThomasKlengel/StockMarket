using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace StockMarket
{
    public static class Logger
    {
        public static void Log(string message)
        {
            string logMessage = $"[{DateTime.Now.ToLongTimeString()}] : message \r\n";

            File.AppendAllText($"Log_{DateTime.Today.ToShortDateString()}.csv", logMessage);

        }

    }
}
