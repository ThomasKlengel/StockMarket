using StockMarket.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace StockMarket.Graphs
{
    /// <summary>
    /// A GraphModel for a <see cref="Share"/>.
    /// </summary>
    public class GraphShareModel : ShareComponentViewModel
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphShareModel"/> class.
        /// </summary>
        public GraphShareModel() : base()
        {
            this.ShareComponents = new ObservableCollection<ShareComponentViewModel>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphShareModel"/> class.
        /// </summary>
        /// <param name="share"></param>
        public GraphShareModel(Share share, double price, DateTime date) : base()
        {
            this.ShareComponents = new ObservableCollection<ShareComponentViewModel>();
            this.baseShare = share;
            this.ShareName = share.ShareName;
            this.WKN = share.WKN;
            this.ISIN = share.ISIN;
            this.IsShare = share.ShareType == ShareType.Share ? true : false;
            this.Factor = this.IsShare == true ? (byte)1 : (byte)10;
            Date = date;
            SetOrdersInitially();
            this.SinglePriceNow = price;
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

        private DateTime _date = DateTime.Today;
        /// <summary>
        /// Gets the current date.
        /// </summary>
        public DateTime Date
        {
            get { return _date; }
            set
            {
                if (_date != value)
                {
                    _date = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Date)));
                }
            }
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
                    if (order.ComponentType == ShareComponentType.Buy || order.ComponentType == ShareComponentType.Dividend)
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

            // add the orders from the database for this user at the given date
            foreach (var order in DataBaseHelper.GetItemsFromDB<Order>(this.baseShare)
                .Where(o => this.SelectByUser(o)).Where(o => o.Date <= Date))
            {
                this.ShareComponents.Add(new OrderViewModel(order));
            }

            // add the dividends from the database for this user
            foreach (var dividend in DataBaseHelper.GetItemsFromDB<Dividend>(this.baseShare)
                  .Where(dividend => this.SelectByUser(dividend)).Where(dividend => dividend.DayOfPayment <= Date))
            {
                this.ShareComponents.Add(new DividendViewModel(dividend));
            }

            // sort the displayed by Date
            this.ShareComponents = this.SortCollection(this.ShareComponents, "Date", false);

            // notify of changes
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Amount)));
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.AmountSold)));
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.SumBuy)));
        }

        /// <inheritdoc/>
        public override void UserChanged()
        {
            this.SetOrdersInitially();
        }
        #endregion

    }
}
