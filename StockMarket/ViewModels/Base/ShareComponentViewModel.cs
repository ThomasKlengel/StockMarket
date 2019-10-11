using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// a base class for share related collections.
    /// </summary>
    public abstract class ShareComponentViewModel : ViewModelBase, IShareComponent
    {
        // TODO: maybe Split up Sorting Method to seperate Class?
        // TODO: maybe set User Stuff to seperate UserViewViewModel (Which implements ViewModelBase)
        #region Fields

        /// <summary>
        /// used for determining the sortdirection when sorting the collection.
        /// </summary>
        public string lastSortedBy;

        /// <summary>
        /// whether the last sort direction was ascending or decending.
        /// </summary>
        public bool lastSortAscending;

        /// <summary>
        /// Gets any item that has <see cref="IHasUserName"/> of the <see cref="CurrentUser"/> (all if DefaultUser).
        /// </summary>
        public Predicate<IHasUserName> SelectByUser;
        #endregion

        #region Contructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShareComponentViewModel"/> class.
        /// </summary>
        public ShareComponentViewModel()
        {
            // set the current user as selected in MainWindow
            ApplicationService.Instance.EventAggregator.GetEvent<UserChangedEvent>().Subscribe((user) => { this.CurrentUser = user; });
        }
        #endregion

        #region Properties

        public double _singlePriceBuy;

        /// <summary>
        /// Gets or sets the price of a single <see cref="Share"/>/<see cref="Order"/>/<see cref="Dividend"/> at the day of purchase.
        /// </summary>
        public virtual double SinglePriceBuy {
    get
    {
        return this._singlePriceBuy;
    }

    set
    {
        if (this._singlePriceBuy != value)
                {
                    this._singlePriceBuy = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.SinglePriceBuy)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.SumBuy)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Difference)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Percentage)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Background)));
                }
    }
        }

        /// <summary>
        /// The price of a single <see cref="Share"/>/<see cref="Order"/>/<see cref="Dividend"/> today.
        /// </summary>
        public double _singlePriceNow;

        /// <inheritdoc/>
        public virtual double SinglePriceNow
        {
            get
            {
                return this._singlePriceNow;
            }

            set
            {
                if (this._singlePriceNow != value)
                {
                    this._singlePriceNow = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.SinglePriceNow)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.SumNow)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Difference)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Percentage)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Background)));
                }
            }
        }

        /// <summary>
        /// Gets the price for the shares today.
        /// </summary>
        public virtual double SumNow
        { get; }

        /// <summary>
        /// Gets the price for the shares at the day bought.
        /// </summary>
        public virtual double SumBuy
        { get; }

        public double _amount;

        /// <summary>
        /// Gets or sets the amount of bought shares.
        /// </summary>
        public virtual double Amount
        {
            get
            {
                return this._amount;
            }

            set
            {
                if (this._amount != value)
                {
                    this._amount = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Amount)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.SumNow)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.SumBuy)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Difference)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Percentage)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Background)));
                }
            }
        }

        /// <summary>
        /// Gets the amount of sold shares.
        /// </summary>
        public virtual double AmountSold
        {
            get;
        }

        /// <summary>
        /// Gets the price difference between buy and today.
        /// </summary>
        public double Difference
        {
            get
            {
                return this.SumNow - this.SumBuy;
            }
        }

        /// <summary>
        /// Gets the background for a share.
        /// </summary>
        public Brush Background
        {
            get
            {
                Color paleRed = (Color)Application.Current.Resources["PaleRed"]; // for negative return
                Color paleGreen = (Color)Application.Current.Resources["PaleGreen"]; // for positive return
                Color paleBlue = (Color)Application.Current.Resources["PaleBlue"]; // for dividends which are always positive return
                Color darkGray = (Color)Application.Current.Resources["DarkGray"]; // for dividends which are always positive return

                if (this.ComponentType == ShareComponentType.Dividend)
                {
                    return new SolidColorBrush(paleBlue);
                }

                var color = this.Percentage > 0.0 ? paleGreen : paleRed;
                // create solid background for shares thet are not completely sold
                Brush solidBack = new SolidColorBrush(color);
                // create partly gray background for shares that are completely sold
                Brush gradientBack = new LinearGradientBrush(darkGray, color, 0);

                return this.Amount - this.AmountSold > 0 ? solidBack : gradientBack;
            }
        }

        /// <summary>
        /// Gets the lost/gained percentage.
        /// </summary>
        public virtual double Percentage
        {
            get { return (this.SumNow / this.SumBuy) - 1.0; }
        }

        private DateTime _bookingDate;

        /// <summary>
        /// Gets or sets the date at which the share item was bought/sold.
        /// </summary>
        public virtual DateTime BookingDate
        {
            get
            {
                return this._bookingDate;
            }

            set
            {
                if (this._bookingDate != value)
                {
                    this._bookingDate = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.BookingDate)));
                }
            }
        }

        public ShareComponentType _componentType = ShareComponentType.Buy;

        /// <summary>
        /// Gets or sets indicates if this Share component was buy, sell or dividend. Used for background settings.
        /// </summary>
        public virtual ShareComponentType ComponentType
        {
            get
            {
                return this._componentType;
            }

            set
            {
                if (this._componentType != value)
                {
                    this._componentType = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.ComponentType)));
                }
            }
        }

        private User _currentUser;

        /// <summary>
        /// Gets the user currently selected in the MainWindow.
        /// </summary>
        public User CurrentUser
        {
            get
            {
                if (this._currentUser != null)
                {
                    return this._currentUser;
                }

                return User.Default();
            }

            private set
            {
                if (this.CurrentUser != value)
                {
                    this._currentUser = value;

                    // Set the delegate to get all Orders of current user
                    if (this.CurrentUser.Equals(User.Default()))
                    {
                        this.SelectByUser = (o) => { return true; };
                    }
                    else
                    {
                        this.SelectByUser = (o) => { return o.UserName == this.CurrentUser.ToString(); };
                    }

                    // Call the method thats ViewModel specific
                    this.UserChanged();
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sorts an <see cref="ObservableCollection{T}"/> by a given property name of the collection items
        /// Mainly used for GridViews.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the <see cref="ObservableCollection{T}"/>s items.</typeparam>
        /// <param name="origCollection">The original <see cref="ObservableCollection{T}"/> to sort.</param>
        /// <param name="sortBy">The name of a property of an item of the <see cref="ObservableCollection{T}"/>.</param>
        /// <param name="ascending">The sort direction (true=ascending, false=descending).</param>
        /// <returns>The sorted <see cref="ObservableCollection{T}"/>.</returns>
        public ObservableCollection<T> SortCollection<T>(ObservableCollection<T> origCollection, string sortBy, bool ascending) where T : ShareComponentViewModel
        {
            #region actual sorting
            // create a copy of the orders
            var tempCollection = new T[origCollection.Count];
            origCollection.CopyTo(tempCollection, 0);

            // create an empty collection
            IOrderedEnumerable<T> sortedCollection = null;

            // rename the sortBy since any sort by Date refers to the BookingDate
            sortBy = sortBy == "Date" ? "BookingDate" : sortBy;
            // get the property to sort by
            System.Reflection.PropertyInfo property = typeof(T).GetProperty(sortBy);

            // sort by property descending or ascending
            if (!ascending) // desceding
            {
                sortedCollection = tempCollection.OrderByDescending((itemOfCollection) => { return property.GetValue(itemOfCollection); });
            }
            else // ascending
            {
                sortedCollection = tempCollection.OrderBy((itemOfCollection) => { return property.GetValue(itemOfCollection); });
            }

            // clear the old orders collection
            origCollection.Clear();

            // add the orders from the sorted collection
            foreach (var item in sortedCollection)
            {
                origCollection.Add(item);
            }
            #endregion

            return origCollection;
        }

        /// <summary>
        /// Any action that should be taken when the <see cref="CurrentUser"/> has changed.
        /// </summary>
        public virtual void UserChanged()
        {
        }

        #endregion

        #region Commands
        public RelayCommand SortCommand { get; protected set; }
        #endregion
    }
}
