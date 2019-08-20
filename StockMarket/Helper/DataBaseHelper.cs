using SQLite;
using StockMarket.DataModels;
using StockMarket.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockMarket
{
    /// <summary>
    /// A helper class for database handling
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

                if (model != null)
                {
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
        /// Adds a <see cref="Share"/> to the database,
        /// also adds a new <see cref="ShareValue"/> for today to the database 
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
                        var shareType = share.IsShare ? ShareType.Share : ShareType.Certificate;
                        con.Insert(new Share(share.ShareName, share.WebSite, share.WKN, share.ISIN, shareType, share.WebSite2, share.WebSite3));
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

        public static short AddUserToDB(User user, string path = DEFAULTPATH)
        {
            if( string.IsNullOrEmpty(user.ToString()) && string.IsNullOrWhiteSpace(user.ToString()))
            {
                return 0;
            }
            try
            {   // connect to the database
                using (SQLiteConnection con = new SQLiteConnection(path))
                {
                    //con.DropTable<User>();

                    // get the required tables of the database
                    con.CreateTable<User>();

                    // check if the share is lready in the database...
                    if (con.Table<User>().ToList().Find((u)=> { return u.ToString() == user.ToString(); })==null)
                    {   //... if not, add it to the tables                       
                        con.Insert(user);
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

        /// <summary>
        /// Adds an <see cref="Order"/> to the database
        /// </summary>
        /// <param name="order">The <see cref="Order"/> to add</param>
        /// <param name="path">The path to the database to insert the <see cref="Share"/>into</param>
        /// <returns>True if successful</returns>
        public static short AddOrderToDB(Order order, string path = DEFAULTPATH)
        {
            try
            {   // connect to the database
                using (SQLiteConnection con = new SQLiteConnection(path))
                {
                    // get the required tables of the database
                    con.CreateTable<Order>();
                    // insert the order
                    con.Insert(order);
                }
                return 1;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public static List<Order> GetOrdersFromDB(Share share, string path = DEFAULTPATH)
        {
            try
            {   // connect to the database
                using (SQLiteConnection con = new SQLiteConnection(path))
                {
                    // get the required tables of the database
                    con.CreateTable<Order>();
                    // insert the order
                    return con.Table<Order>().ToList().FindAll((order) => { return order.ISIN == share.ISIN; });
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<Order> GetOrdersFromDB(string isin, string path = DEFAULTPATH)
        {
            try
            {   // connect to the database
                using (SQLiteConnection con = new SQLiteConnection(path))
                {
                    // get the required tables of the database
                    con.CreateTable<Order>();
                    // retrun the orders matching the ISIN
                    return con.Table<Order>().ToList().FindAll((order) => { return order.ISIN == isin; });
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<Order> GetAllOrdersFromDB(string path = DEFAULTPATH)
        {
            try
            {   // connect to the database
                using (SQLiteConnection con = new SQLiteConnection(path))
                {
                    // get the required tables of the database
                    con.CreateTable<Order>();

                    var a = con.Table<Order>().ToList();
                    // retrun the orders matching the ISIN
                    return con.Table<Order>().ToList();
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<Share> GetSharesFromDB(string path = DEFAULTPATH)
        {
            try
            {   // connect to the database
                using (SQLiteConnection con = new SQLiteConnection(path))
                {
                    // get the required tables of the database
                    con.CreateTable<Share>();
                    // return the table as list, orderd by the ShareName
                    return con.Table<Share>().ToList().OrderBy((s) => { return s.ShareName; }).ToList(); ;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<ShareValue> GetShareValuesFromDB(Share share, string path = DEFAULTPATH)
        {
            try
            {   // connect to the database
                using (SQLiteConnection con = new SQLiteConnection(path))
                {
                    // get the required tables of the database
                    con.CreateTable<ShareValue>();
                    // insert the order
                    return con.Table<ShareValue>().ToList().FindAll((val) => { return val.ISIN == share.ISIN; });
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<ShareValue> GetShareValuesFromDB(string isin, string path = DEFAULTPATH)
        {
            try
            {   // connect to the database
                using (SQLiteConnection con = new SQLiteConnection(path))
                {
                    // get the required tables of the database
                    con.CreateTable<ShareValue>();
                    // get the values
                    return con.Table<ShareValue>().ToList().FindAll((val) => { return val.ISIN == isin; });
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<User> GetUsersFromDB(string path = DEFAULTPATH)
        {
            try
            {   // connect to the database
                using (SQLiteConnection con = new SQLiteConnection(path))
                {
                    // get the required tables of the database
                    con.CreateTable<User>();
                    // insert the order
                    return con.Table<User>().ToList();
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Adds an <see cref="Order"/> to the database
        /// </summary>
        /// <param name="order">The <see cref="Order"/> to add</param>
        /// <param name="path">The path to the database to insert the <see cref="Share"/>into</param>
        /// <returns>True if successful</returns>
        public static short AddShareValueToDB(ShareValue share, string path = DEFAULTPATH)
        {
            try
            {   // connect to the database
                using (SQLiteConnection con = new SQLiteConnection(path))
                {
                    // get the required tables of the database
                    con.CreateTable<ShareValue>();
                    // insert the order
                    con.Insert(share);
                }
                return 1;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

    }
}
