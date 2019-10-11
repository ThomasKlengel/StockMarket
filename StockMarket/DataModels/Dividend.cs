using System;
using SQLite;

namespace StockMarket
{
    /// <summary>
    /// A class containing basic information about a dividend payment of a <see cref="Share"/>.
    /// </summary>
    public class Dividend : IHasIsin, IHasUserName
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Dividend"/> class.
        /// </summary>
        public Dividend()
        {
            this.UserName = new User().ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dividend"/> class.
        /// </summary>
        /// <param name="isin"></param>
        /// <param name="dayOfPayment"></param>
        /// <param name="paymentValue"></param>
        /// <param name="amountOfShares"></param>
        /// <param name="dateRangeStart"></param>
        /// <param name="dateRangeEnd"></param>
        public Dividend(string isin, DateTime dayOfPayment, double paymentValue, double amountOfShares, DateTime dateRangeStart, DateTime dateRangeEnd)
        {
            this.ISIN = isin;
            this.DayOfPayment = dayOfPayment;
            this.Value = paymentValue;
            this.Amount = amountOfShares;
            this.DateRangeStart = dateRangeStart;
            this.DateRangeEnd = dateRangeEnd;
        }

        [PrimaryKey][AutoIncrement]
        /// <summary>
        /// The primary key for the Database; auto incremented
        /// </summary>
        public int DB_ID { get; set; }

        /// <summary>
        /// Gets or sets the ISIN of the share for the dividend payment.
        /// </summary>
        public string ISIN { get; set; }

        /// <summary>
        /// Gets or sets the day of the dividend payment.
        /// </summary>
        public DateTime DayOfPayment { get; set; }

        /// <summary>
        /// Gets or sets the dividend payed.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the amount of shares for which the dividen is payed.
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Gets or sets the start date of the period of time for which the dividend is payed.
        /// </summary>
        public DateTime DateRangeStart { get; set; }

        /// <summary>
        /// Gets or sets the end date of the period of time for which the dividend is payed.
        /// </summary>
        public DateTime DateRangeEnd { get; set; }

        /// <inheritdoc/>
        public string UserName { get; set; }
    }
}
