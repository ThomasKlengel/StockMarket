using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for all <see cref="Order"/>s of a <see cref="Share"/>
    /// </summary>
    class OrderOverviewViewModel:ViewModelBase
    {
        #region Properties
               
        /// <summary>
        /// The average share price for the orders
        /// </summary>
        public double AvgSharePrice
        {
            get
            {
                double sum = 0;
                foreach (var order in Orders)
                {
                    sum += order.SharePrice;
                };
                return sum / Orders.Count;
            }

        }

        private double _actPrice;

        /// <summary>
        /// The current share price for the orders
        /// </summary>
        public double ActPrice
        {
            get { return _actPrice; }
            set
            {
                _actPrice = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ActPrice)));
            }
        }

        /// <summary>
        /// The amount of shares in all orders
        /// </summary>
        public int Amount
        {
            get
            {
                int sum = 0;
                foreach (var order in Orders)
                {
                    sum += order.Amount;
                };
                return sum;
            }
        }

        /// <summary>
        /// The current date
        /// </summary>
        public DateTime Date
        {
            get { return DateTime.Today; }            
        }

        /// <summary>
        /// The summed up price for all orders on the date of purchase
        /// </summary>
        public double SumBuy
        {
            get
            {
                double sum = 0;
                foreach (var order in Orders)
                {
                    sum += order.Amount * order.SharePrice;
                };
                return sum;
            }
        }

        /// <summary>
        /// The current summed up price for all orders 
        /// </summary>
        public double SumNow
        {
            get
            {
                double sum = 0;
                foreach (var order in Orders)
                {
                    sum += order.Amount * ActPrice;
                };
                return sum;
            }
        }

        /// <summary>
        /// The background color for the overview determined by 
        /// a positive or negative development of share prices
        /// </summary>
        public SolidColorBrush Backgropund
        {
            get { return SumNow - SumBuy > 0 ? new SolidColorBrush(Color.FromRgb(222, 255, 209)) : new SolidColorBrush(Color.FromRgb(255, 127, 127)); }
        }

        /// <summary>
        /// The development of share prices in percent
        /// </summary>
        public double Percentage
        {
            get { return SumNow / SumBuy - 1.0; }
        }

        /// <summary>
        /// All orders of the selected share
        /// </summary>
        public List<OrderViewModel> Orders { get; set; }

        #endregion        
    }
}
