using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using StockMarket.ViewModels;

namespace StockMarket
{
    /// <summary>
    /// A helper class for database handling.
    /// </summary>
    public static class DataBaseHelper
    {
        /// <summary>
        /// the usual path to the database.
        /// </summary>
        private const string DEFAULTPATH = "Share.DB";

        /// <summary>
        /// Saves the application configuration to a database.
        /// </summary>
        /// <param name="Model">The data to save to the database.</param>
        /// <param name="path">The path to the database (Default = "Share.DB").</param>
        public static void SaveToDB(SharesDataModel model, string path = DEFAULTPATH)
        {
            // create a connection to the database
            using (SQLiteConnection con = new SQLiteConnection(path))
            {
                // create the tabels
                con.CreateTable<Share>();
                con.CreateTable<Order>();
                con.CreateTable<ShareValue>();

                // remove all entries for the tables
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
                            WKN = s.WKN,
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
                            SharePrice = o.SharePrice,
                        }, typeof(Order));
                    }

                    foreach (var v in model.ShareValues)
                    {   // populate the share table with the values of the datamodel
                        con.Insert(new ShareValue()
                        {
                            Date = v.Date,
                            ISIN = v.ISIN,
                            Price = v.Price,
                        }, typeof(ShareValue));
                    }
                }
            }
        }

        /// <summary>
        /// Reads the application configuration from the database.
        /// </summary>
        /// <param name="path">The path to the database (Default = "Share.DB").</param>
        /// <returns>The data to read from the database.</returns>
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
        /// also adds a new <see cref="ShareValue"/> for today to the database.
        /// </summary>
        /// <param name="share">The <see cref="Share"/> to add.</param>
        /// <param name="path">The path to the database to insert the <see cref="Share"/>into.</param>
        /// <returns>1 if successful, 0 if a share matching the ISIN already exists, -1 if an error occured.</returns>
        public static short AddShareToDB(AddShareViewModel share, string path = DEFAULTPATH)
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
                    {   // ... if not, add it to the tables
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
                Logger.Log("AddShareToDB : "+ ex.Message);
                return -1;
            }
        }

        /// <summary>
        /// Modifies the values Website/2/3 and ShareName of an existing <see cref="Share"/> within the database.
        /// </summary>
        /// <param name="modify">The <see cref="Share"/> with the modified values.</param>
        /// <param name="path">The path to the database.</param>
        /// <returns>1 if successful, 0 if no share matching the ISIN existed, -1 if an error occured.</returns>
        public static short ModifiyShare(Share modify, string path = DEFAULTPATH)
        {
            try
            {   // connect to the database
                using (SQLiteConnection con = new SQLiteConnection(path))
                {
                    // get the required tables of the database
                    con.CreateTable<Share>();
                    // get the according share from the database...
                    var existingShare = con.Find<Share>(modify.ISIN);

                    // set values
                    if (existingShare != null)
                    {
                        existingShare.WebSite = modify.WebSite;
                        existingShare.WebSite2 = modify.WebSite2;
                        existingShare.WebSite3 = modify.WebSite3;
                        existingShare.ShareName = modify.ShareName;
                        // modify them in the database as well
                        con.RunInTransaction(() =>
                        {
                            con.Update(existingShare);
                        });
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
                Logger.Log("ModifyShare : " + ex.Message);
                return -1;
            }
        }

        public static short AddUserToDB(User user, string path = DEFAULTPATH)
        {
            if ( string.IsNullOrEmpty(user.ToString()) && string.IsNullOrWhiteSpace(user.ToString()))
            {
                return 0;
            }

            try
            {   // connect to the database
                using (SQLiteConnection con = new SQLiteConnection(path))
                {
                    // con.DropTable<User>();

                    // get the required tables of the database
                    con.CreateTable<User>();

                    // check if the share is lready in the database...
                    if (con.Table<User>().ToList().Find((u) => { return u.ToString() == user.ToString(); }) == null)
                    {   // ... if not, add it to the tables
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
                Logger.Log("AddUserToDB : " + ex.Message);
                return -1;
            }
        }

        /// <summary>
        /// Adds an <see cref="Order"/> to the database.
        /// </summary>
        /// <param name="order">The <see cref="Order"/> to add.</param>
        /// <param name="path">The path to the database to insert the <see cref="Order"/>into.</param>
        /// <returns>True if successful.</returns>
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
                Logger.Log("AddOrderToDB : " + ex.Message);
                return -1;
            }
        }

        /// <summary>
        /// Adds an <see cref="Dividend"/> to the database.
        /// </summary>
        /// <param name="dividend">The <see cref="Dividend"/> to add.</param>
        /// <param name="path">The path to the database to insert the <see cref="Dividend"/>into.</param>
        /// <returns>True if successful.</returns>
        public static short AddDividendToDB(Dividend dividend, string path = DEFAULTPATH)
        {
            try
            {   // connect to the database
                using (SQLiteConnection con = new SQLiteConnection(path))
                {
                    // get the required tables of the database
                    con.CreateTable<Dividend>();
                    // insert the order
                    con.Insert(dividend);
                }

                return 1;
            }
            catch (Exception)
            {
                return -1;
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
                    return con.Table<Share>().ToList().OrderBy((s) => { return s.ShareName; }).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("GetSharesFromDB : " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Gets any share component of the specified type with the defined isin from the database or all if isin is string.Empty.
        /// </summary>
        /// <typeparam name="T">Any share component that implements <see cref="IHasIsin"/>.</typeparam>
        /// <param name="isin">The isin to match.</param>
        /// <param name="path">The path to the database.</param>
        /// <returns></returns>
        public static List<T> GetItemsFromDB<T>(string isin, string path = DEFAULTPATH) where T : IHasIsin, new()
        {
            try
            {   // connect to the database
                using (SQLiteConnection con = new SQLiteConnection(path))
                {
                    // get the required table of the database
                    con.CreateTable<T>();
                    // get the values
                    return con.Table<T>().ToList().FindAll((val) => { return isin != string.Empty ? val.ISIN == isin : true; });
                }
            }
            catch (Exception ex)
            {
                Logger.Log("GetItemsFromDB(isin) : " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Gets any share component of the specified type with an ISIN matching the defined share from the database.
        /// </summary>
        /// <typeparam name="T">Any share component that implements <see cref="IHasIsin"/>.</typeparam>
        /// <param name="share">The <see cref="Share"/> to match.</param>
        /// <param name="path">The path to the database.</param>
        /// <returns></returns>
        public static List<T> GetItemsFromDB<T>(Share share, string path = DEFAULTPATH) where T : IHasIsin, new()
        {
            try
            {
                return GetItemsFromDB<T>(share.ISIN);
            }
            catch (Exception ex)
            {
                Logger.Log("GetItemsFromDB(share) : " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Gets all items of the specified type from the database.
        /// </summary>
        /// <typeparam name="T">Any share component that implements <see cref="IHasIsin"/>.</typeparam>
        /// <param name="path">The path to the database.</param>
        /// <returns></returns>
        public static List<T> GetAllItemsFromDB<T>(string path = DEFAULTPATH) where T : IHasIsin, new()
        {
            try
            {
                return GetItemsFromDB<T>(string.Empty);
            }
            catch (Exception ex)
            {
                Logger.Log("GetAllItemsFromDB : " + ex.Message);
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
                Logger.Log("GetUsersFromDB : " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Adds an <see cref="Order"/> to the database.
        /// </summary>
        /// <param name="order">The <see cref="Order"/> to add.</param>
        /// <param name="path">The path to the database to insert the <see cref="Share"/>into.</param>
        /// <returns>True if successful.</returns>
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
                Logger.Log("AddShareValueToDB : " + ex.Message);
                return -1;
            }
        }
    }
}
