using SQLite;
using StockMarket.DataModels;
using StockMarket.ViewModels;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace StockMarket
{
    /// <summary>
    /// A helper classe for database handling
    /// </summary>
    public static class DataBaseHelper
    {
        /// <summary>
        /// the usual path to the database
        /// </summary>
        private const string DEFAULTPATH = "Share.DB";

        /// <summary>
        /// Saves the application configuration to a database
        /// </summary>
        /// <param name="Model">The data to save to the database</param>
        /// <param name="path">The path to the database (Default = "Share.DB")</param>
        public static void SaveToDB(SharesDataModel model, string path = DEFAULTPATH)
        {
            //create a connection to the database
            using (SQLiteConnection con = new SQLiteConnection(path))
            {
                //create the tabels
                con.CreateTable<Share>();
                con.CreateTable<Order>();
                con.CreateTable<ShareValue>();

                //remove all entries for the tables
                con.DeleteAll<Share>();
                con.DeleteAll<Order>();
                con.DeleteAll<ShareValue>();
                                
                foreach (var s in model.Shares)
                {   // populate the share table with the values of the datamodel
                    con.Insert(new Share()
                    {
                        ISIN = s.ISIN,
                        ShareName = s.ShareName,
                        WebSite = s.WebSite,
                        WKN = s.WKN
                    }, typeof(Share));
                }
                                
                foreach (var o in model.Orders)
                {   // populate the order table with the values of the datamodel
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
                {   // populate the share table with the values of the datamodel
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
            // create a new datamodel
            SharesDataModel model = new SharesDataModel();
            
            // connect to the database
            using (SQLiteConnection con = new SQLiteConnection(path))
            {
                // get the tables of the database
                con.CreateTable<Share>();
                con.CreateTable<Order>();
                con.CreateTable<ShareValue>();

                // populate the model with the table contents
                model.Shares = con.Table<Share>().ToList();
                model.Orders = con.Table<Order>().ToList();
                model.ShareValues = con.Table<ShareValue>().ToList();
            }

            return model;
        }

        /// <summary>
        /// Adds a <see cref="Share"/> to the database
        /// </summary>
        /// <param name="share">The <see cref="Share"/> to add</param>
        /// <param name="path">The path to the database to insert the <see cref="Share"/>into</param>
        /// <returns>True if successful</returns>
        public static short AddShareToDB(ShareViewModel share, string path = DEFAULTPATH)
        {
            try
            {   // connect to the database
                using (SQLiteConnection con = new SQLiteConnection(path))
                {
                    // get the required tables of the database
                    con.CreateTable<Share>();
                    con.CreateTable<ShareValue>();                    
                    
                    // check if the share is lready in the database...
                    if (con.Find<Share>(share.ISIN) == null)
                    {   //... if not, add it to the tables
                        con.Insert(new Share(share.ShareName, share.WebSite, share.WKN, share.ISIN));
                        con.Insert(new ShareValue() { Date = DateTime.Now, ISIN = share.ISIN, Price = share.ActualPrice });
                    }
                    else
                    {
                        return 0;
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }
    }

    public static class RegexHelper
    {
        #region Regex strings
        public const string REGEX_SharePrice = "\\d*\\.?\\d*,\\d*";
        public const string REGEX_Group_SharePrice = "\\<tr\\>\\<td class=\"font-bold\"\\>Kurs\\<.*EUR.*\\<span";
        public const string REGEX_Group_IDs = "instrument-id\"\\>.{40}";
        public const string REGEX_ISIN = "ISIN: \\S{12}";
        public const string REGEX_WKN = "WKN: .{6}";
        public const string REGEX_Group_ShareName = "box-headline\"\\>Aktienkurs.{50}";
        public const string REGEX_ShareName = "Aktienkurs .* in";
        public const string REGEX_ISIN_Valid = "^\\S{12}$";
        public const string REGEX_IsShare = "^https:\\/{2}w{3}\\.finanzen\\.net\\/aktien\\/.+-Aktie$";
        public const string REGEX_IsOption = "^https:\\/{2}w{3}\\.finanzen\\.net\\/optionsscheine\\/Auf-.+\\/.{6}$";
        public const string REGEX_Website_Valid = "^https:\\/{2}w{3}\\.finanzen\\.net.+$";
        #endregion

        /// <summary>
        /// Gets the price of a share from a string
        /// </summary>
        /// <param name="input">the string to check for the price</param>
        /// <returns>the price of the share</returns>
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
 
        /// <summary>
        /// Checks if a website is valid for handling by this programm
        /// </summary>
        /// <param name="website">the website to check</param>
        /// <returns>true if the website is valid</returns>
        public static bool WebsiteIsValid(string website)
        {            
            return Regex.Match(website, REGEX_Website_Valid).Success;
        }

        /// <summary>
        /// Checks if an ISIN is valid for handling by this programm
        /// </summary>
        /// <param name="website">the ISIN to check</param>
        /// <returns>true if the ISIN is valid</returns>
        public static bool IsinIsValid(string isin)
        {            
            return Regex.Match(isin, REGEX_ISIN_Valid).Success;
        }

    }
}
