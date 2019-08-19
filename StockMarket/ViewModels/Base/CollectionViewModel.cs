using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// a base class for share related collections
    /// </summary>
    public abstract class CollectionViewModel: ViewModelBase
    {
        #region Delegates
        public delegate bool OrderSelector(Order o);
        #endregion

        #region Fields
        /// <summary>
        /// used for determining the sortdirection when sorting the collection
        /// </summary>        
        public string lastSortedBy;
        /// <summary>
        /// whether the last sort direction was ascending or decending
        /// </summary>
        public bool lastSortAscending;
        /// <summary>
        /// Gets the orders of the <see cref="CurrentUser"/> (all if DefaultUser)
        /// </summary>
        public OrderSelector SelectOrderByUser;

        private User _currentUser;
        /// <summary>
        /// The user currently selected in the MainWindow
        /// </summary>
        public User CurrentUser
        {
            get
            {
                if (_currentUser!= null)
                {
                    return _currentUser;
                }
                return User.Default();
            }

            private set
            {
                if (CurrentUser != value)
                {
                    _currentUser = value;

                    //Set the delegate to get all Orders of current user
                    if (CurrentUser.Equals(User.Default()))
                    {
                        SelectOrderByUser = (o) => { return true; };
                    }
                    else
                    {                        
                        SelectOrderByUser = (o) => { return o.UserName == CurrentUser.ToString(); };
                    }

                    // Call the method thats ViewModel specific
                    UserChanged();
                }
            }
        }
        #endregion

        #region Contructors
        public CollectionViewModel()
        {            
            // set the current user as selected in MainWindow
            ApplicationService.Instance.EventAggregator.GetEvent<UserChangedEvent>().Subscribe((user)=> { CurrentUser = user; });
        }
        #endregion

        #region Properties
        /// <summary>
        /// The price for the shares today
        /// </summary>
        public abstract double SumNow
        { get; }

        /// <summary>
        /// The price for the shares at the day bought
        /// </summary>
        public abstract double SumBuy
        { get; }

        /// <summary>
        /// The amount of bought shares
        /// </summary>
        public abstract int Amount
        {
            get; set;
        }

        /// <summary>
        /// The amount of sold shares
        /// </summary>
        public abstract int AmountSold
        {
            get; 
        }

        /// <summary>
        /// The price difference between buy and today
        /// </summary>
        public double Difference
        {
            get
            {
                return SumNow - SumBuy;
            }
        }

        /// <summary>
        /// The background for a share 
        /// </summary>
        public Brush Background
        {
            get
            {
                var paleRed = Color.FromRgb(255, 127, 127);
                var paleGreen = Color.FromRgb(222, 255, 209);
                var color = Percentage > 0.0 ? paleGreen : paleRed;
                // create solid background for shares thet are not completely sold
                Brush solidBack = new SolidColorBrush(color);
                // create partly gray background for shares that are completely sold
                Brush gradientBack = new LinearGradientBrush(Colors.Gray, color, 0);

                return Amount-AmountSold > 0 ? solidBack : gradientBack;
            }
        }

        /// <summary>
        /// The lost/gained percentage
        /// </summary>
        public double Percentage
        {
            get { return SumNow / SumBuy - 1.0; }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Sorts an <see cref="ObservableCollection{T}"/> by a given property name of the collection items
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the <see cref="ObservableCollection{T}"/>s items</typeparam>
        /// <param name="origCollection">The original <see cref="ObservableCollection{T}"/> to sort</param>
        /// <param name="sortBy">The name of a property of an item of the <see cref="ObservableCollection{T}"/></param>
        /// <param name="ascending">The sort direction (true=ascending, false=descending)</param>
        /// <returns>The sorted <see cref="ObservableCollection{T}"/></returns>
        public ObservableCollection<T> SortCollection<T>(ObservableCollection<T> origCollection, string sortBy, bool ascending)
        {
            #region actual sorting
            // create a copy of the orders
            var tempCollection = new T[origCollection.Count];
            origCollection.CopyTo(tempCollection, 0);

            // create an empty collection
            IOrderedEnumerable<T> sortedCollection = null;

            // get the property to sort by
            System.Reflection.PropertyInfo property = typeof(T).GetProperty(sortBy);

            // sort by property descending or ascending
            if (!ascending) //desceding
            {
                sortedCollection = tempCollection.OrderByDescending((itemOfCollection) => { return property.GetValue(itemOfCollection); });
            }
            else //ascending
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
        /// Any action that should be taken when the <see cref="CurrentUser"/> has changed
        /// </summary>
        public virtual void UserChanged() { }

        #endregion

        #region Commands
        public RelayCommand SortCommand { get; protected set; }
        #endregion
    }
}
