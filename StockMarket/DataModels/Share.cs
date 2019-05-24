using System;
using SQLite;

namespace StockMarket
{    
    /// <summary>
    /// A class containing basic information about a share
    /// </summary>
    public class Share
    {
        #region ctors
        public Share() { }

        public Share(string name, string website, string wkn, string isin)
        {
            ShareName = name;
            WebSite = website;
            WKN = wkn;
            ISIN = isin;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The name of the stock company
        /// </summary>
        public string ShareName { get; set; }

        /// <summary>
        /// The website from which to get the data for the <see cref="Share"/>
        /// </summary>
        public string WebSite { get; set; }

        /// <summary>
        /// Thw WKN of the <see cref="Share"/>
        /// </summary>
        public string WKN { get; set; }

        /// <summary>
        /// The ISIN of the <see cref="Share"/>
        /// </summary>
        [PrimaryKey]
        public string ISIN { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Checks the equality of two shares by their ISIN
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Share))
            {
                return this.ISIN == (obj as Share).ISIN;
            }
            return false;
        }
        #endregion

    }

    /// <summary>
    /// A class containing basic information about a <see cref="Share"/> order
    /// </summary>
    public class Order
    {
        #region ctors
        public Order() { }
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
        public int Amount { get; set; }

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
        public OrderType OrderType { get; set; }

        /// <summary>
        /// The ISIN of the order to link to a share
        /// </summary>
        public string ISIN { get; set; }

        #endregion
    }

    /// <summary>
    /// A class containing information about a <see cref="Share"/> price at a specific date
    /// </summary>
    public class ShareValue
    {
        #region ctors
        public ShareValue() { }
        #endregion

        #region Properties

        /// <summary>
        /// The primary key for the Database; auto incremented
        /// </summary>
        [PrimaryKey]
        [AutoIncrement]
        public int DB_ID { get; set; }

        /// <summary>
        /// The price of a <see cref="Share"/> at the date of purchase
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// The date of the price
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The ISIN of the <see cref="ShareValue"/> to link to a <see cref="Share"/>
        /// </summary>
        public string ISIN { get; set; }
        #endregion

    }

    /// <summary>
    /// An enumeration of the type of order
    /// (buy = 1;
    /// sell = -1)
    /// </summary>
    public enum OrderType
    {
        buy = 1,
        sell = -1
    }
}
