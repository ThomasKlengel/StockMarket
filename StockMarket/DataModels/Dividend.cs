﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace StockMarket.DataModels
{
    /// <summary>
    /// A class containing basic information about a dividend payment of a <see cref="Share"/>
    /// </summary>
    class Dividend
    {

        public Dividend() { }

        public Dividend(string isin, DateTime dayOfPayment, double paymentValue, int amountOfShares, DateTime dateRangeStart, DateTime dateRangeEnd)
        {
            ISIN = isin;
            DayOfPayment = dayOfPayment;
            Value = paymentValue;
            Amount = amountOfShares;
            DateRangeStart = dateRangeStart;
            DateRangeEnd = dateRangeEnd;
        }


        [PrimaryKey][AutoIncrement]
        /// <summary>
        /// The primary key for the Database; auto incremented
        /// </summary>
        public int DB_ID { get; set; }

        /// <summary>
        /// The ISIN of the share for the dividend payment
        /// </summary>
        public string ISIN { get; set; }

        /// <summary>
        /// The day of the dividend payment
        /// </summary>
        public DateTime DayOfPayment { get; set; }

        /// <summary>
        /// The dividend payed
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// The amount of shares for which the dividen is payed
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// The start date of the period of time for which the dividend is payed
        /// </summary>
        public DateTime DateRangeStart { get; set; }

        /// <summary>
        /// The end date of the period of time for which the dividend is payed
        /// </summary>
        public DateTime DateRangeEnd { get; set; }
        
    }
}
