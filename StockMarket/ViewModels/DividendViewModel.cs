using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.Collections.Generic;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for a single <see cref="Order"/>
    /// </summary>
    public class DividendViewModel : CollectionViewModel
    {
        #region ctor
        /// <summary>
        /// Creates a ViewModel for an order with an AddOrderCommand 
        /// </summary>
        public DividendViewModel() : base()
        {
          
        }

        /// <summary>
        /// Creates a ViewModel for an order from an order 
        /// </summary>
        /// <param name="order">The <see cref="Order"/> to create a ViewModel for</param>
        public DividendViewModel(Dividend dividend) : base()
        {
            this.Amount = dividend.Amount;
            this.BookingDate = dividend.DayOfPayment;
            this.ComponentType =  ShareComponentType.dividend;
            this.SinglePriceBuy = dividend.Value/dividend.Amount;
            this.ISIN = dividend.ISIN;
            this.UserName = dividend.UserName;
        }

        #endregion

        #region Properties

        public string UserName { get; private set; }

        private readonly string ISIN;

        override public int AmountSold
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// The summed up price of the shares at the day of purchase
        /// (getter only)
        /// </summary>
        override public double SumBuy
        {
            get
            {
                return 0.0;
            }
        }

        /// <summary>
        /// The current summed up price of the shares
        /// (getter only)
        /// </summary>
        override public double SumNow
        {
            get
            {
                return Amount * SinglePriceBuy;
            }
        }

        public override double Percentage
        {
            get { return SumNow / Amount / SinglePriceNow; }
        }
        #endregion


    }
}

