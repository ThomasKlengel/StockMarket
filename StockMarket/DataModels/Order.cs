using System;
using SQLite;

namespace StockMarket
{
    /// <summary>
    /// A class containing basic information about a <see cref="Share"/> order.
    /// </summary>
    public class Order : IHasIsin, IHasUserName
    {
        #region ctors

        /// <summary>
        /// Initializes a new instance of the <see cref="Order"/> class.
        /// </summary>
        public Order()
        {
            this.UserName = new User().ToString();
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the primary key for the Database; auto incremented.
        /// </summary>
        [PrimaryKey]
        [AutoIncrement]
        public int DB_ID { get; set; }

        /// <summary>
        /// Gets or sets the price of the <see cref="Share"/> at the date of purchase.
        /// </summary>
        public double SharePrice { get; set; }

        /// <summary>
        /// Gets or sets the amount of <see cref="Share"/>s purchased.
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Gets or sets the date of purchase.
        /// </summary>
        public DateTime Date { get; set; }

        //TODO: Add Taxes
        /// <summary>
        /// Gets or sets the fee for the purchase.
        /// </summary>
        public double OrderExpenses { get; set; }

        /// <summary>
        /// Gets or sets type of order (buy/sell).
        /// </summary>
        public ShareComponentType OrderType { get; set; }

        /// <summary>
        /// Gets or sets the ISIN of the order to link to a share.
        /// </summary>
        public string ISIN { get; set; }

        /// <inheritdoc/>
        public string UserName { get; set; }

        public bool ReInvesting { get; set; }
        #endregion
    }

    /// <summary>
    /// An enumeration of the type of order
    /// (buy = 1;
    /// dividend = 0;
    /// sell = -1).
    /// </summary>
    public enum ShareComponentType
    {
        /// <summary>
        /// An order where shares were bought (1)
        /// </summary>
        Buy = 1,

        /// <summary>
        /// A Dividend (0)
        /// </summary>
        Dividend = 0,

        /// <summary>
        /// An order where shares were sold (-1)
        /// </summary>
        Sell = -1,
    }
}
