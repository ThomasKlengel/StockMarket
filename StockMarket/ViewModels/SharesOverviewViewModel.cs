using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.Linq;
using System.Windows.Threading;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for all <see cref="Shares"/> in the <see cref="Pages.SharesOverviewPage"/>
    /// </summary>
    class SharesOverviewViewModel : ViewModelBase
    {

        public SharesOverviewViewModel()
        {
            Shares = new List<ShareOverviewViewModel>();
            var shares = DataBaseHelper.GetSharesFromDB();
            foreach (var share in shares)
            {
                Shares.Add(new ShareOverviewViewModel(share));
            }
        }

        public List<ShareOverviewViewModel> Shares { get; private set; }

        /// <summary>
        /// The summed up price for all orders on the date of purchase
        /// </summary>
        public double SumBuy
        {
            get
            {
                double buy = 0;

                foreach (var share in Shares)
                {
                    buy += share.SumBuy;
                }
                return buy;
            }
        }

        /// <summary>
        /// The current summed up price for all orders 
        /// </summary>
        public double SumNow
        {
            get
            {
                double buy = 0;

                foreach (var share in Shares)
                {
                    buy += share.SumNow;
                }
                return buy;
            }
        }

        public int Amount
        {
            get
            {
                int amount = 0;
                foreach (var share in Shares)
                {
                    amount += share.Amount;
                }

                return amount;
            }
        }

        /// <summary>
        /// The background color for the overview determined by 
        /// a positive or negative development of share prices
        /// </summary>
        public SolidColorBrush Backgropund
        {
            get
            {
                return SumNow - SumBuy > 0 ? new SolidColorBrush(Color.FromRgb(222, 255, 209))
                                           : new SolidColorBrush(Color.FromRgb(255, 127, 127));
            }
        }

        /// <summary>
        /// The development of share prices in percent
        /// </summary>
        public double Percentage
        {
            get { return SumNow / SumBuy - 1.0; }
        }
    }

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
            RefreshPriceAsync();
        }

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
                var Orders = DataBaseHelper.GetOrdersFromDB(ISIN);
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
                var Orders = DataBaseHelper.GetOrdersFromDB(ISIN);
                double buy = 0;

                foreach (var o in Orders)
                {
                    if (o.OrderType == OrderType.buy)
                    {
                        buy +=  o.Amount * o.SharePrice + 2 * o.OrderExpenses;
                    }
                }
                return buy;
            }
        }

        /// <summary>
        /// The current summed up price for all orders 
        /// </summary>
        public double SumNow
        {
            get
            {
                var Orders = DataBaseHelper.GetOrdersFromDB(ISIN).OrderBy((o)=> { return o.Date; });
                int notSold=0;
                double sell=0;
                
                foreach (var o in Orders)
                {
                    if (o.OrderType == OrderType.buy)
                    {
                        notSold+=o.Amount;
                    }
                    else
                    {
                        sell+=o.Amount * o.SharePrice;
                        notSold -= o.Amount;                        
                    }
                }

                sell += notSold * ActualPrice;
                return sell;
            }
        }

        /// <summary>
        /// The background color for the overview determined by 
        /// a positive or negative development of share prices
        /// </summary>
        public SolidColorBrush Backgropund
        {
            get
            {
                return SumNow - SumBuy > 0 ? new SolidColorBrush(Color.FromRgb(222, 255, 209))
                                           : new SolidColorBrush(Color.FromRgb(255, 127, 127));
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

        public void refreshPrice()
        {
            RefreshPriceAsync();
        } 
        #endregion        
    }
}
