using SQLite;
using System;

namespace StockMarket
{
    /// <summary>
    /// A class containing information about a <see cref="Share"/> price at a specific date
    /// </summary>
    public class ShareValue: IHasIsin
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
}
