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
            Task.Run(async () =>
            {
                var errorOccured = true;
                while (errorOccured)
                {
                    try
                    {
                        if (!Directory.Exists("logs"))
                        {
                            Directory.CreateDirectory("logs");
                        }

                        string logMessage = $"[{DateTime.Now.ToString("HH:mm:ss.fff")}] : {message} \r\n";
                        File.AppendAllText($"logs\\Log_{DateTime.Today.ToString("yyyy-MM-dd")}.csv", logMessage);
                        errorOccured = false;
                    }
                    catch (Exception ex)
                    {
                        errorOccured = true;
                        Task.Delay(100).Wait();
                    }
                }
            });

        }

    }
}
