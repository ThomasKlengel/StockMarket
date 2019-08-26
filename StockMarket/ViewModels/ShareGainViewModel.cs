using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for a single <see cref="Share"/> in the <see cref="Pages.SharesOverviewPage"/>
    /// </summary>
    class ShareGainViewModel : CollectionViewModel
    {

        #region Constructors
        public ShareGainViewModel():base()
        {

        }

        /// <summary>
        /// Creates a <see cref="ShareGainViewModel"/> for a given <see cref="Share"/>
        /// </summary>
        /// <param name="share">The <see cref="Share"/> to create the view model for</param>
        public ShareGainViewModel(Share share):base()
        {
            ShareName = share.ShareName;
            WebSite = share.WebSite;
            WebSite2 = share.WebSite2;
            WebSite3 = share.WebSite3;
            WKN = share.WKN;
            ISIN = share.ISIN;
            ShareName = share.ShareName;
            ShareType = share.ShareType;
            GetOrders();
            GetPriceAsync();
        }
        #endregion

        #region Properties     
        /// <summary>
        /// All orders of this share
        /// </summary>
        public ObservableCollection<OrderViewModel> Orders { get; private set; }
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

        private string _webSite2;
        /// <summary>
        /// The website from which to get the data for the <see cref="Share"/>
        /// </summary>
        public string WebSite2
        {
            get { return _webSite2; }
            set
            {
                if (_webSite2 != value)
                {
                    _webSite2 = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(WebSite2)));
                }
            }
        }

        private string _webSite3;
        /// <summary>
        /// The website from which to get the data for the <see cref="Share"/>
        /// </summary>
        public string WebSite3
        {
            get { return _webSite3; }
            set
            {
                if (_webSite3 != value)
                {
                    _webSite3 = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(WebSite3)));
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


        /// <summary>
        /// The current price for the share
        /// </summary>
        public override double SinglePriceNow 
        {
            get =>  base.SinglePriceNow; 
            set
            {
                if (_singlePriceNow != value)
                {
                    _singlePriceNow = value;
                    foreach (var o in Orders)
                    {
                        o.SinglePriceNow = _singlePriceNow;
                    }
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SinglePriceNow)));
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
                    if (o.ComponentType == ShareComponentType.buy)
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
                    if (o.ComponentType == ShareComponentType.sell)
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
        private async void GetPriceAsync()
        {
            //set the price for the UI
            SinglePriceNow = await RegexHelper.GetSharePriceAsync(new Share(ShareName, WebSite, WKN, ISIN, ShareType, WebSite2, WebSite3));
        }

        /// <summary>
        /// Gets all orders for this share from the database and adds them to <see cref="Orders"/>
        /// </summary>
        private void GetOrders()
        {
            // get the orders from the databse
            var sortedOrders = DataBaseHelper.GetItemsFromDB<Order>(ISIN).OrderByDescending((o) => { return o.Date; });

            // create or clear the list of Orders
            if (Orders == null)
            {
                Orders = new ObservableCollection<OrderViewModel>();
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
