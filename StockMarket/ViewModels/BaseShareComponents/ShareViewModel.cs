using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for a <see cref="Share"/>
    /// </summary>
    class ShareViewModel : ShareComponentViewModel
    {

        #region Constructors
        public ShareViewModel():base()
        {
            ShareComponents = new ObservableCollection<ShareComponentViewModel>();
        }

        public ShareViewModel(Share share) : base()
        {
            ShareComponents = new ObservableCollection<ShareComponentViewModel>();
            baseShare = share;
            ShareName = share.ShareName;
            WebSite = share.WebSite;
            WebSite2 = share.WebSite2;
            WebSite3 = share.WebSite3;
            WKN = share.WKN;
            ISIN = share.ISIN;
            IsShare = share.ShareType == ShareType.Share ? true : false;
            Factor = IsShare == true ? (byte)1  : (byte)10;

        }

        #endregion

        #region Properties

        /// <summary>
        /// The current share price for the orders
        /// </summary>
        public override double SinglePriceNow
        {
            get { return base.SinglePriceNow; }
            set
            {
                if (_singlePriceNow != value)
                {
                    foreach (var order in ShareComponents)
                    {
                        order.SinglePriceNow = value;
                    }
                }
                base.SinglePriceNow = value;                
            }
        }

        /// <summary>
        /// The amount of shares in all orders
        /// </summary>
        override public int Amount
        {
            get
            {
                int sum = 0;
                foreach (var order in ShareComponents)
                {
                    if (order.ComponentType == ShareComponentType.buy)
                    {
                        sum += order.Amount;
                    }
                };
                return sum;
            }
            set { return; }
        }

        /// <summary>
        /// The amount of shares sold over all orders
        /// </summary>
        override public int AmountSold
        {
            get
            {
                int sum = 0;
                foreach (var order in ShareComponents)
                {
                    if (order.ComponentType == ShareComponentType.sell)
                    {
                        sum += order.Amount;
                    }
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
        override public double SumBuy
        {
            get
            {
                double sum = 0;

                foreach (var shareComponent in ShareComponents)
                {
                    // we can ignors sells, because sells have no "buy" costs
                    // we can also ignore dividends because divends have no "buy" costs either
                    if (shareComponent.ComponentType == ShareComponentType.buy)
                    {
                        sum += shareComponent.SumBuy;
                    }
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
                foreach (var order in ShareComponents)
                {
                    // we dont need to take sells into account since sells are represented in the SumNow of a buy if any amount of it was sold
                    if (order.ComponentType == ShareComponentType.buy  || order.ComponentType== ShareComponentType.dividend)
                    {
                        sum += order.SumNow;
                    }
                };
                return sum;
            }
        }

        /// <summary>
        /// All orders of the selected share
        /// </summary>
        public ObservableCollection<ShareComponentViewModel> ShareComponents { get; set; }

        #region Propties of Share itself (from Share Data model)
        private readonly Share baseShare;

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

        private bool _shareTypeIsShare = true;
        public bool IsShare
        {
            get { return _shareTypeIsShare; }
            set
            {
                if (value)
                {
                    _shareTypeIsShare = true;
                }
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsShare)));
            }
        }

        public bool IsCertificate
        {
            get { return !_shareTypeIsShare; }
            set
            {
                if (value)
                {
                    _shareTypeIsShare = false;
                }
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsCertificate)));
            }
        }

        private byte _factor;
        public byte Factor
        {
            get { return _factor; }
            set
            {
                if (_factor != value)
                {
                    _factor = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Factor)));
                }
            }
        }

        #endregion
        #endregion

        #region Methods
        /// <summary>
        /// Selects the <see cref="ShareComponents"/> associated to the selected <see cref="Share"/>
        /// and add them to the <see cref="ShareComponents"/> property
        /// </summary>
        private void SetOrdersInitially()
        {
            // create or clear the list of Orders
            if (ShareComponents == null)
            {
                ShareComponents = new ObservableCollection<ShareComponentViewModel>();
            }
            ShareComponents.Clear();


            // add the orders from the database for this user  
            foreach (var order in DataBaseHelper.GetItemsFromDB<Order>(baseShare)
                .Where(o => SelectByUser(o)))
            {
                ShareComponents.Add(new OrderViewModel(order));
            }

            // add the dividends from the database for this user
            foreach (var dividend in DataBaseHelper.GetItemsFromDB<Dividend>(baseShare)
                  .Where(dividend => SelectByUser(dividend)))
            {
                ShareComponents.Add(new DividendViewModel(dividend));
            }

            //sort the displayed by Date
            ShareComponents = SortCollection(ShareComponents, "Date", false);

            // notify UI of changes
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Amount)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(AmountSold)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(SumBuy)));

            RefreshPriceAsync();

        }

        /// <summary>
        /// refreshes the actual <see cref="Share"/> price
        /// </summary>
        private async void RefreshPriceAsync()
        {
            var price = await RegexHelper.GetSharePriceAsync(baseShare);

            //set the price for the UI
            SinglePriceNow = price;
        }

        public override void UserChanged()
        {
            SetOrdersInitially();
        }
        #endregion

        #region Commands

        private void SortOrders(object o)
        {
            if (ShareComponents.Count > 1)
            {
                // check if clicked item is a column header
                if (o.GetType() == typeof(GridViewColumnHeader))
                {
                    var header = o as GridViewColumnHeader;

                    var headerClicked = "";
                    // if the binding is a binding...
                    if (header.Column.DisplayMemberBinding.GetType() == typeof(Binding))
                    { //... set the header to the bound path
                        var content = header.Column.DisplayMemberBinding as Binding;
                        headerClicked = content.Path.Path;
                        if (headerClicked.Contains("Date"))
                        {
                            headerClicked = "Date";
                        }
                    }
                    else
                    { //... otherwise it's amount (which is a multibinding)
                        headerClicked = "Amount";
                    }

                    //get the sort Direction
                    if (lastSortedBy == headerClicked)
                    {
                        lastSortAscending = !lastSortAscending;
                    }
                    else
                    {
                        lastSortAscending = false;
                    }

                    //sort the orders
                    ShareComponents = SortCollection(ShareComponents, headerClicked, lastSortAscending);
                   
                    // set the last sorted by for next sort
                    lastSortedBy = headerClicked;

                }
            }
        }

        #endregion

    }
}
