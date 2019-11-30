using System;
using SQLite;

namespace StockMarket
{
    /// <summary>
    /// A class containing information about a <see cref="Share"/> price at a specific date.
    /// </summary>
    public class ShareValue : IHasIsin
    {
        #region ctors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShareValue"/> class.
        /// </summary>
        public ShareValue()
        {
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
        /// Gets or sets the price of a <see cref="Share"/> at the date of purchase.
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// Gets or sets the date of the price.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the ISIN of the <see cref="ShareValue"/> to link to a <see cref="Share"/>.
        /// </summary>
        public string ISIN { get; set; }
        #endregion

    }
}
