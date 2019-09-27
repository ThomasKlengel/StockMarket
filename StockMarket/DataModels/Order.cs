using SQLite;
using System;

namespace StockMarket
{
    /// <summary>
    /// A class containing basic information about a <see cref="Share"/> order
    /// </summary>
    public class Order: IHasIsin, IHasUserName
    {
        #region ctors
        public Order() { UserName = new User().ToString(); }
        #endregion

        #region Properties

        /// <summary>
        /// The primary key for the Database; auto incremented
        /// </summary>
        [PrimaryKey]
        [AutoIncrement]
        public int DB_ID { get; set; }

        /// <summary>
        /// The price of the <see cref="Share"/> at the date of purchase
        /// </summary>
        public double SharePrice { get; set; }

        /// <summary>
        /// The amount of <see cref="Share"/>s purchased
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// The date of purchase
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The fee for the purchase
        /// </summary>
        public double OrderExpenses { get; set; }

        /// <summary>
        /// Type of order (buy/sell)
        /// </summary>
        public ShareComponentType OrderType { get; set; }

        /// <summary>
        /// The ISIN of the order to link to a share
        /// </summary>
        public string ISIN { get; set; }

        public string UserName { get; set; }        

        public bool ReInvesting { get; set; }
        #endregion
    }


    /// <summary>
    /// An enumeration of the type of order
    /// (buy = 1;
    /// dividend = 0;
    /// sell = -1)
    /// </summary>
    public enum ShareComponentType
    {
        buy = 1,
        dividend = 0,
        sell = -1
    }
}
