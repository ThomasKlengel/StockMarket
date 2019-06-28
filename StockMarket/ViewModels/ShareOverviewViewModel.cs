﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for a single <see cref="Share"/> in the <see cref="Pages.SharesOverviewPage"/>
    /// </summary>
    class ShareOverviewViewModel : ViewModelBase
    {
        #region Properties

        public ShareOverviewViewModel()
        {

        }

        public ShareOverviewViewModel(Share share)
        {
            ShareName = share.ShareName;
            WebSite = share.WebSite;
            WKN = share.WKN;
            ISIN = share.ISIN;
            ShareName = share.ShareName;
            GetOrders();
            RefreshPriceAsync();
            
        }

        private List<OrderViewModel> Orders;

        private string _shareName;
        /// <summary>
        /// The name of the stock company
        /// </summary>
        public string ShareName
        {
            get { return _shareName; }
            set
            {
                _shareName = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ShareName)));
            }
        }

        private string _webSite;
        /// <summary>
        /// The website from which to get the data for the <see cref="Share"/>
        /// </summary>
        public string WebSite
        {
            get { return _webSite; }
            set
            {
                _webSite = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(WebSite)));
            }
        }

        private string _wkn;
        /// <summary>
        /// Thw WKN of the <see cref="Share"/>
        /// </summary>
        public string WKN
        {
            get { return _wkn; }
            set
            {
                _wkn = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(WKN)));
            }
        }

        private string _isin;
        /// <summary>
        /// The ISIN of the <see cref="Share"/>
        /// </summary>
        public string ISIN
        {
            get { return _isin; }
            set
            {
                _isin = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ISIN)));
            }
        }

        private double _actPrice;
        /// <summary>
        /// The current price for the share
        /// </summary>
        public double ActualPrice
        {
            get { return _actPrice; }
            set
            {
                _actPrice = value;
                foreach(var o in Orders)
                {
                    o.ActPrice = _actPrice;
                }
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ActualPrice)));
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SumNow)));
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Percentage)));
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Backgropund)));
            }
        }

        /// <summary>
        /// The amount of shares in all orders
        /// </summary>
        public int Amount
        {
            get
            {
                int amount = 0;

                foreach (var o in Orders)
                {
                    if (o.OrderType == OrderType.buy)
                    {
                        amount += o.Amount;
                    }
                    else
                    {
                        amount -= o.Amount;
                    }
                }
                return amount;
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
                    sum += order.SumBuy;
                }
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
                    sum += order.SumNow;
                };
                return sum;
            }
        }

        /// <summary>
        /// The background color for the overview determined by 
        /// a positive or negative development of share prices
        /// </summary>
        public Brush Backgropund
        {
            get
            {
                var paleRed = Color.FromRgb(255, 127, 127);
                var paleGreen = Color.FromRgb(222, 255, 209);
                var color = Percentage > 0.0 ? paleGreen : paleRed;
                Brush solidBack = new SolidColorBrush(color);
                Brush gradientBack = new LinearGradientBrush(Colors.Gray, color, 0);

                return Amount > 0 ? solidBack : gradientBack;
            }
        }

        /// <summary>
        /// The development of share prices in percent (as a fraction of 1.0)
        /// </summary>
        public double Percentage
        {
            get { return SumNow / SumBuy - 1.0; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// refreshes the actual <see cref="Share"/> price
        /// </summary>
        private async void RefreshPriceAsync()
        {
            // get the website content
            var content = await WebHelper.getWebContent(WebSite);
            //get the price
            var price = RegexHelper.GetSharPrice(content);
            //set the price for the UI
            this.ActualPrice = price;
        }

        //public void refreshPrice()
        //{
        //    RefreshPriceAsync();
        //}

        private void GetOrders()
        {
            // get the orders from the databse
            var sortedOrders = DataBaseHelper.GetOrdersFromDB(ISIN).OrderByDescending((o) => { return o.Date; });

            // create or clear the list of Orders
            if (Orders == null)
            {
                Orders = new List<OrderViewModel>();
            }
            Orders.Clear();

            // add the orders from the database
            foreach (var order in sortedOrders)
            {
                Orders.Add(new OrderViewModel(order));
            }
        }
        #endregion
    }
}