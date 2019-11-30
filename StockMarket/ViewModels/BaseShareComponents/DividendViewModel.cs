using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for a single <see cref="Order"/>.
    /// </summary>
    public class DividendViewModel : ShareComponentViewModel
    {
        #region ctor

        /// <summary>
        /// Creates a ViewModel for an order with an AddOrderCommand.
        /// </summary>
        public DividendViewModel()
            : base()
        {
        }

        /// <summary>
        /// Creates a ViewModel for an order from an order.
        /// </summary>
        /// <param name="dividend">The <see cref="Dividend"/> to create a ViewModel for.</param>
        public DividendViewModel(Dividend dividend)
            : base()
        {
            this.Amount = dividend.Amount;
            this.BookingDate = dividend.DayOfPayment;
            this.ComponentType =  ShareComponentType.Dividend;
            this.SinglePriceBuy = dividend.Value / dividend.Amount;
            this.ISIN = dividend.ISIN;
            this.UserName = dividend.UserName;
        }

        #endregion

        #region Properties

        public string UserName { get; private set; }

        private readonly string ISIN;

        /// <inheritdoc/>
        public override double AmountSold
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the summed up price of the shares at the day of purchase
        /// (getter only).
        /// </summary>
        public override double SumBuy
        {
            get
            {
                return 0.0;
            }
        }

        /// <summary>
        /// Gets the current summed up price of the shares
        /// (getter only).
        /// </summary>
        public override double SumNow
        {
            get
            {
                return this.Amount * this.SinglePriceBuy;
            }
        }

        /// <inheritdoc/>
        public override double Percentage
        {
            get { return this.SumNow / this.Amount / this.SinglePriceNow; }
        }
        #endregion

    }
}

