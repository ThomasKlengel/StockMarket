using System;
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
    class ShareOverviewViewModel : CollectionViewModel
    {
        #region ctor
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
            ShareType = share.ShareType;
            GetOrders();
            RefreshPriceAsync();

        }
        #endregion

        #region Properties               
        private List<OrderViewModel> Orders;
        public ShareType ShareType { get; private set; }

        private string _shareName;
        /// <summary>
        /// The name of the stock company
        /// </summary>
        public string ShareName
        {
            get { return _shareName; }
            set
            {
                if (_shareName != value)
                {
                    _shareName = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ShareName)));
                }
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
                if (_webSite != value)
                {
                    _webSite = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(WebSite)));
                }
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
                if (_wkn != value)
                {
                    _wkn = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(WKN)));
                }
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
                if (_isin != value)
                {
                    _isin = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ISIN)));
                }
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
                if (_actPrice != value)
                {
                    _actPrice = value;
                    foreach (var o in Orders)
                    {
                        o.ActPrice = _actPrice;
                    }
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ActualPrice)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SumNow)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Difference)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Percentage)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Background)));
                }
            }
        }

        /// <summary>
        /// The amount of shares in all orders
        /// </summary>
        override public int Amount
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
                }
                return amount;
            }
            set { return; }
        }

        public override int AmountSold
        {
            get
            {
                int amount = 0;
                foreach (var o in Orders)
                {
                    if (o.OrderType == OrderType.sell)
                    {
                        amount += o.AmountSold;
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
        override public double SumBuy
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
        override public double SumNow
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
            var price = RegexHelper.GetSharePrice(content, ShareType);
            //set the price for the UI
            this.ActualPrice = price;
        }

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
