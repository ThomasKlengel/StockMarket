using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for a <see cref="Share"/>.
    /// </summary>
    public class ShareViewModel : ShareComponentViewModel
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShareViewModel"/> class.
        /// </summary>
        public ShareViewModel()
            : base()
        {
            this.ShareComponents = new ObservableCollection<ShareComponentViewModel>();
            this.SortCommand = new RelayCommand(this.SortComponents);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShareViewModel"/> class.
        /// </summary>
        /// <param name="share"></param>
        public ShareViewModel(Share share)
            : base()
        {
            this.ShareComponents = new ObservableCollection<ShareComponentViewModel>();
            this.SortCommand = new RelayCommand(this.SortComponents);
            this.baseShare = share;
            this.ShareName = share.ShareName;
            this.WebSite = share.WebSite;
            this.WebSite2 = share.WebSite2;
            this.WebSite3 = share.WebSite3;
            this.WKN = share.WKN;
            this.ISIN = share.ISIN;
            this.IsShare = share.ShareType == ShareType.Share ? true : false;
            this.Factor = this.IsShare == true ? (byte)1  : (byte)10;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the current share price for the orders.
        /// </summary>
        public override double SinglePriceNow
        {
            get
            {
                return base.SinglePriceNow;
            }

            set
            {
                if (this._singlePriceNow != value)
                {
                    foreach (var order in this.ShareComponents)
                    {
                        order.SinglePriceNow = value;
                    }
                }

                base.SinglePriceNow = value;
            }
        }

        /// <summary>
        /// Gets or sets the amount of shares in all orders.
        /// </summary>
        public override double Amount
        {
            get
            {
                double sum = 0;
                foreach (var order in this.ShareComponents)
                {
                    if (order.ComponentType == ShareComponentType.Buy)
                    {
                        sum += order.Amount;
                    }
                }

                return sum;
            }

            set
            {
                return;
            }
        }

        /// <summary>
        /// Gets the amount of shares sold over all orders.
        /// </summary>
        public override double AmountSold
        {
            get
            {
                double sum = 0;
                foreach (var order in this.ShareComponents)
                {
                    if (order.ComponentType == ShareComponentType.Sell)
                    {
                        sum += order.Amount;
                    }
                }

                return sum;
            }
        }

        /// <summary>
        /// Gets the current date.
        /// </summary>
        public DateTime Date
        {
            get { return DateTime.Today; }
        }

        /// <summary>
        /// Gets the summed up price for all orders on the date of purchase.
        /// </summary>
        public override double SumBuy
        {
            get
            {
                double sum = 0;

                foreach (var shareComponent in this.ShareComponents)
                {
                    // we can ignors sells, because sells have no "buy" costs
                    // we can also ignore dividends because divends have no "buy" costs either
                    if (shareComponent.ComponentType == ShareComponentType.Buy)
                    {
                        sum += shareComponent.SumBuy;
                    }
                }

                return sum;
            }
        }


        //TODO: recalculate the current value
        /// <summary>
        /// Gets the current summed up price for all orders.
        /// </summary>
        public override double SumNow
        {
            get
            {
                double sum = 0;
                foreach (var order in this.ShareComponents)
                {
                    // we dont need to take sells into account since sells are represented in the SumNow of a buy if any amount of it was sold
                    if (order.ComponentType == ShareComponentType.Buy  || order.ComponentType == ShareComponentType.Dividend)
                    {
                        sum += order.SumNow;
                    }
                }

                return sum;
            }
        }

        /// <summary>
        /// Gets or sets all orders of the selected share.
        /// </summary>
        public ObservableCollection<ShareComponentViewModel> ShareComponents { get; set; }

        #region Propties of Share itself (from Share Data model)
        private readonly Share baseShare;

        private string _shareName;

        /// <summary>
        /// Gets or sets the name of the stock company.
        /// </summary>
        public string ShareName
        {
            get
            {
                return this._shareName;
            }

            set
            {
                if (this._shareName != value)
                {
                    this._shareName = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.ShareName)));
                }
            }
        }

        private string _webSite;

        /// <summary>
        /// Gets or sets the website from which to get the data for the <see cref="Share"/>.
        /// </summary>
        public string WebSite
        {
            get
            {
                return this._webSite;
            }

            set
            {
                if (this._webSite != value)
                {
                    this._webSite = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.WebSite)));
                }
            }
        }

        private string _webSite2;

        /// <summary>
        /// Gets or sets the website from which to get the data for the <see cref="Share"/>.
        /// </summary>
        public string WebSite2
        {
            get
            {
                return this._webSite2;
            }

            set
            {
                if (this._webSite2 != value)
                {
                    this._webSite2 = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.WebSite2)));
                }
            }
        }

        private string _webSite3;

        /// <summary>
        /// Gets or sets the website from which to get the data for the <see cref="Share"/>.
        /// </summary>
        public string WebSite3
        {
            get
            {
                return this._webSite3;
            }

            set
            {
                if (this._webSite3 != value)
                {
                    this._webSite3 = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.WebSite3)));
                }
            }
        }

        private string _wkn;

        /// <summary>
        /// Gets or sets thw WKN of the <see cref="Share"/>.
        /// </summary>
        public string WKN
        {
            get
            {
                return this._wkn;
            }

            set
            {
                if (this._wkn != value)
                {
                    this._wkn = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.WKN)));
                }
            }
        }

        private string _isin;

        /// <summary>
        /// Gets or sets the ISIN of the <see cref="Share"/>.
        /// </summary>
        public string ISIN
        {
            get
            {
                return this._isin;
            }

            set
            {
                if (this._isin != value)
                {
                    this._isin = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.ISIN)));
                }
            }
        }

        private bool _shareTypeIsShare = true;

        public bool IsShare
        {
            get
            {
                return this._shareTypeIsShare;
            }

            set
            {
                if (value)
                {
                    this._shareTypeIsShare = true;
                }

                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.IsShare)));
            }
        }

        public bool IsCertificate
        {
            get
            {
                return !this._shareTypeIsShare;
            }

            set
            {
                if (value)
                {
                    this._shareTypeIsShare = false;
                }

                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.IsCertificate)));
            }
        }

        private byte _factor;

        public byte Factor
        {
            get
            {
                return this._factor;
            }

            set
            {
                if (this._factor != value)
                {
                    this._factor = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Factor)));
                }
            }
        }

        #endregion
        #endregion

        #region Methods

        /// <summary>
        /// Selects the <see cref="ShareComponents"/> associated to the selected <see cref="Share"/>
        /// and add them to the <see cref="ShareComponents"/> property.
        /// </summary>
        private void SetOrdersInitially()
        {
            // create or clear the list of Orders
            if (this.ShareComponents == null)
            {
                this.ShareComponents = new ObservableCollection<ShareComponentViewModel>();
            }

            this.ShareComponents.Clear();

            // add the orders from the database for this user
            foreach (var order in DataBaseHelper.GetItemsFromDB<Order>(this.baseShare)
                .Where(o => this.SelectByUser(o)))
            {
                this.ShareComponents.Add(new OrderViewModel(order));
            }

            // add the dividends from the database for this user
            foreach (var dividend in DataBaseHelper.GetItemsFromDB<Dividend>(this.baseShare)
                  .Where(dividend => this.SelectByUser(dividend)))
            {
                this.ShareComponents.Add(new DividendViewModel(dividend));
            }

            // sort the displayed by Date
            this.ShareComponents = this.SortCollection(this.ShareComponents, "Date", false);

            // notify UI of changes
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Amount)));
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.AmountSold)));
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.SumBuy)));

            this.RefreshPriceAsync();
        }

        /// <summary>
        /// refreshes the actual <see cref="Share"/> price.
        /// </summary>
        private async void RefreshPriceAsync()
        {
            await Task.Run(async() =>
            {
                var price = await RegexHelper.GetSharePriceAsync(this.baseShare);

                // set the price for the UI
                this.SinglePriceNow = price;
            });

        }

        /// <inheritdoc/>
        public override void UserChanged()
        {
            this.SetOrdersInitially();
        }
        #endregion

        #region Commands

        private void SortComponents(object o)
        {
            if (this.ShareComponents.Count > 1)
            {
                // check if clicked item is a column header
                if (o.GetType() == typeof(GridViewColumnHeader))
                {
                    var header = o as GridViewColumnHeader;

                    var headerClicked = string.Empty;
                    // if the binding is a binding...
                    if (header.Column.DisplayMemberBinding.GetType() == typeof(Binding))
                    { // ... set the header to the bound path
                        var content = header.Column.DisplayMemberBinding as Binding;
                        headerClicked = content.Path.Path;
                        if (headerClicked.Contains("Date"))
                        {
                            headerClicked = "Date";
                        }
                    }
                    else
                    { // ... otherwise it's amount (which is a multibinding)
                        headerClicked = "Amount";
                    }

                    // get the sort Direction
                    if (this.lastSortedBy == headerClicked)
                    {
                        this.lastSortAscending = !this.lastSortAscending;
                    }
                    else
                    {
                        this.lastSortAscending = false;
                    }

                    // sort the orders
                    this.ShareComponents = this.SortCollection(this.ShareComponents, headerClicked, this.lastSortAscending);

                    // set the last sorted by for next sort
                    this.lastSortedBy = headerClicked;
                }
            }
        }

        #endregion

    }
}
