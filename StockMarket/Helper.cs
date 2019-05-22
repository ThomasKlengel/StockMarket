using SQLite;
using StockMarket.DataModels;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace StockMarket
{
    public static class DataBaseHelper
    {
        private const string DEFAULTPATH = "Share.DB";

        /// <summary>
        /// Saves the application configuration to a database
        /// </summary>
        /// <param name="Model">The data to save to the database</param>
        /// <param name="path">The path to the database (Default = "Share.DB")</param>
        public static void SaveToDB(SharesDataModel model, string path = DEFAULTPATH)
        {
            using (SQLiteConnection con = new SQLiteConnection(path))
            {
                con.CreateTable<Share>();
                con.CreateTable<Order>();
                con.CreateTable<ShareValue>();

                con.DeleteAll<Share>();
                con.DeleteAll<Order>();
                con.DeleteAll<ShareValue>();

                foreach (var s in model.Shares)
                {
                    con.Insert(new Share()
                    {
                        ISIN = s.ISIN,
                        ShareName = s.ShareName,
                        WebSite = s.WebSite,
                        WKN = s.WKN
                    }, typeof(Share));
                }

                foreach (var o in model.Orders)
                {
                    con.Insert(new Order()
                    {
                        ISIN = o.ISIN,
                        Amount = o.Amount,
                        Date = o.Date,
                        OrderExpenses = o.OrderExpenses,
                        OrderType = o.OrderType,
                        SharePrice = o.SharePrice
                    }, typeof(Order));
                }

                foreach (var v in model.ShareValues)
                {
                    con.Insert(new ShareValue()
                    {
                        Date = v.Date,
                        ISIN = v.ISIN,
                        Price = v.Price
                    }, typeof(ShareValue));

                }
            }            
        }

        /// <summary>
        /// Reads the application configuration from the database
        /// </summary>
        /// <param name="path">The path to the database (Default = "Share.DB")</param>
        /// <returns>The data to read from the database</returns>
        public static SharesDataModel ReadFromDB(string path = DEFAULTPATH)
        {
            SharesDataModel model = new SharesDataModel();
            
            using (SQLiteConnection con = new SQLiteConnection(path))
            {
                con.CreateTable<Share>();
                con.CreateTable<Order>();
                con.CreateTable<ShareValue>();

                model.Shares = con.Table<Share>().ToList();
                model.Orders = con.Table<Order>().ToList();
                model.ShareValues = con.Table<ShareValue>().ToList();
            }

            return model;
        }
    }

    public static class RegexHelper
    {
        public const string REGEX_SharePrice = "\\d*\\.?\\d*,\\d*";
        public const string REGEX_Group_SharePrice = "\\<tr\\>\\<td class=\"font-bold\"\\>Kurs\\<.*EUR.*\\<span";
        public const string REGEX_Group_IDs = "instrument-id\"\\>.{40}";
        public const string REGEX_ISIN = "ISIN: \\S{2}\\d{10}";
        public const string REGEX_WKN = "WKN: \\d{6}";
        public const string REGEX_Group_ShareName = "box-headline\"\\>Aktienkurs.{50}";
        public const string REGEX_ShareName = "Aktienkurs .* in";

        public static double GetSharPrice (string input)
        {
            // get the section of the website which contains the SharePrice
            //<tr><td class="font-bold">Kurs</td><td colspan="4">18,25 EUR<span
            var priceMatch = Regex.Match(input, RegexHelper.REGEX_Group_SharePrice);
            if (!priceMatch.Success)
            {
                return 0.0;
            }
            // get the SharePrice in the desired format
            string sharePrice = Regex.Match(priceMatch.Value, RegexHelper.REGEX_SharePrice).Value.Replace(".", "");

            return Convert.ToDouble(sharePrice, CultureInfo.GetCultureInfo("de-DE"));
        }
    }
}
