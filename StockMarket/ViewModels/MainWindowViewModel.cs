using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// a view model for the main window.
    /// </summary>
    class MainWindowViewModel : ViewModels.ViewModelBase
    {
        /// <summary>
        /// creates a new instance of a ViewModel for the MainWindow.
        /// </summary>
        public MainWindowViewModel()
        {

            // TODO: ShareOverview add overview tiles sold, shares, certificates
            // TODO: single share Graph

            // set the start page to an empty page
            this.DisplayPage = new Pages.BlankPage();

            // define the commands for the buttons
            this.AddUserCommand = new RelayCommand(this.AddUser);
            this.AddShareCommand = new RelayCommand(this.AddShare);
            this.AddDividendCommand = new RelayCommand(this.AddDividend);
            this.AddOrderCommand = new RelayCommand(this.AddOrder,this.CanAddOrder);
            this.DisplaySingleShareOverviewCommand = new RelayCommand(this.DisplaySingleShareOverview, this.CanDisplaySingleShareOverview);
            this.DisplaySharesOverviewCommand = new RelayCommand(this.DisplaySharesOverview, this.CanDisplaySharesOverview);
            this.DisplayShareDetailCommand = new RelayCommand(this.DisplayShareDetail, this.CanDisplayShareDetail);

            // create a timer for updating the Sharevalues in the database
            DispatcherTimer t = new DispatcherTimer
            {
                Interval = new System.TimeSpan(0, 20, 0),
            };
            t.Tick += this.TimerTick;
            t.Start();

            // try to update share values once at program start
            this.TimerTick(null, null);

            // set the initial Users
            if (this.Users == null)
            {
                this.Users = new ObservableCollection<User>();
            }
            
            foreach (var user in DataBaseHelper.GetUsersFromDB())
            {
                this.Users.Add(user);
            }

            this.CurrentUser = this.Users.First((u) => { return u.Equals(User.Default()); });
        }

        #region EventHandler

        /// <summary>
        /// Eventhandler for updating the Sharevalues in the database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TimerTick(object sender, System.EventArgs e)
        {
            // if its a friday night...
            if (/*DateTime.Now.DayOfWeek == DayOfWeek.Friday*/
                DateTime.Now.DayOfWeek != DayOfWeek.Saturday &&
                DateTime.Now.DayOfWeek != DayOfWeek.Sunday &&
                DateTime.Now.Hour >= 22)
            {
                // ... get all shares in the portfolio
                var shares = DataBaseHelper.GetAllItemsFromDB<Share>();

                // remove the shares which where no share is currently purchased
                //for (int i = shares.Count - 1;i >= 0;i--)
                //{
                //    var orders = DataBaseHelper.GetItemsFromDB<Order>(shares[i]);

                //    double amountRemaining = 0;
                //    foreach (var o in orders)
                //    {

                //        if (o.OrderType == ShareComponentType.Buy)
                //        {
                //            amountRemaining += o.Amount;
                //        }
                //        else if (o.OrderType == ShareComponentType.Sell)
                //        {
                //            amountRemaining -= o.Amount;
                //        }
                //    }

                //    if (amountRemaining < 1)
                //    {
                //        shares.RemoveAt(i);
                //    }
                //}

                // for each of these shares...
                foreach (var share in shares)
                {
                    // ...get the latest value in the database
                    var latestValues = DataBaseHelper.GetItemsFromDB<ShareValue>(share)?.OrderByDescending((v) => v.Date);
                    if (latestValues.Count() > 0)
                    {
                        var latestValue = latestValues.First();
                        // if it is from today...
                        if (latestValue?.Date.Date == DateTime.Today)
                        {   // ... we can ignore the following and continue with the next share
                            continue;
                        }
                    }

                    var price = await RegexHelper.GetSharePriceAsync(share);
                    
                    // create a new sharevalue
                    ShareValue s = new ShareValue()
                    {
                        Date = DateTime.Today,
                        ISIN = share.ISIN,
                        Price = price,
                    };

                    // and add it to the database
                    DataBaseHelper.AddShareValueToDB(s);
                }
            }
        }
        #endregion

        #region Properties
        private Page _displayPage;

        /// <summary>
        /// Gets the <see cref="Page"/> to display in the main frame.
        /// </summary>
        public Page DisplayPage
        {
            get
            {
                return this._displayPage;
            }

            private set
            {
                if (this._displayPage != value)
                {
                    this._displayPage = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.DisplayPage)));
                }
            }
        }

        public ObservableCollection<User> Users
        {
            get; private set;
        }

        private User _currentUser;

        public User CurrentUser
        {
            get
            {
                return this._currentUser;
            }

            set
            {
                if (this._currentUser != value)
                {
                    this._currentUser = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.CurrentUser)));
                    ApplicationService.Instance.EventAggregator.GetEvent<UserChangedEvent>().Publish(this.CurrentUser);
                }
            }
        }
        #endregion

        #region Commands

        #region Add User Command
        public RelayCommand AddUserCommand { get; private set; }

        private void AddUser(object o)
        {
            this.DisplayPage = new Pages.AddUserPage();
        }
        #endregion

        #region Add Share Command
        public RelayCommand AddShareCommand { get; private set; }

        private void AddShare(object o)
        {
            this.DisplayPage = new Pages.AddSharePage();
            ApplicationService.Instance.EventAggregator.GetEvent<UserChangedEvent>().Publish(this.CurrentUser);
        }
        #endregion

        #region Add Dividend Command
        public RelayCommand AddDividendCommand { get; private set; }

        private void AddDividend(object o)
        {
            this.DisplayPage = new Pages.AddDividendPage();
            ApplicationService.Instance.EventAggregator.GetEvent<UserChangedEvent>().Publish(this.CurrentUser);
        }
        #endregion

        #region Add Order Command
        public RelayCommand AddOrderCommand { get; private set; }

        private void AddOrder(object o)
        {
            this.DisplayPage = new Pages.AddOrderPage();
            ApplicationService.Instance.EventAggregator.GetEvent<UserChangedEvent>().Publish(this.CurrentUser);
        }

        private bool CanAddOrder(object o)
        {
            int shares = 0;
            // try-catch only for UI generator
            try
            {
                shares = DataBaseHelper.GetSharesFromDB().Count;
            }
            catch
            {
                return true;
            }

            // execute only when we have at least one share
            return shares > 0;
        }
        #endregion

        #region Display Share Detail Command
        public RelayCommand DisplayShareDetailCommand { get; private set; }

        private void DisplayShareDetail(object o)
        {
            this.DisplayPage = new Pages.ShareDetailPage();
            ApplicationService.Instance.EventAggregator.GetEvent<UserChangedEvent>().Publish(this.CurrentUser);
        }

        private bool CanDisplayShareDetail(object o)
        {
            int count = 0;
            // try-catch only for UI generator
            try
            {
                count = DataBaseHelper.GetSharesFromDB().Count;
            }
            catch
            {
                return true;
            }

            // execute only when we have at least one share
            return count > 0;
        }
        #endregion

        #region Display Single Share Gain Command
        public RelayCommand DisplaySingleShareOverviewCommand { get; private set; }

        private void DisplaySingleShareOverview(object o)
        {
            this.DisplayPage = new Views.ShareGainPage();
            ApplicationService.Instance.EventAggregator.GetEvent<UserChangedEvent>().Publish(this.CurrentUser);
        }

        private bool CanDisplaySingleShareOverview(object o)
        {
            int orders = 0;

            // try-catch only for UI generator
            try
            {
                var shares = DataBaseHelper.GetSharesFromDB();
                foreach (var share in shares)
                {
                    orders += DataBaseHelper.GetItemsFromDB<Order>(share).Count;
                }
            }
            catch
            {
                return true;
            }

            // execute only when we have at least one order of any share
            return orders > 0;
        }
        #endregion

        #region Display Shares Gain Command
        public RelayCommand DisplaySharesOverviewCommand { get; private set; }

        private void DisplaySharesOverview(object o)
        {
            this.DisplayPage = new Pages.SharesOverviewPage();
            ApplicationService.Instance.EventAggregator.GetEvent<UserChangedEvent>().Publish(this.CurrentUser);
        }

        private bool CanDisplaySharesOverview(object o)
        {
            int count = 0;
            // try-catch only for UI generator
            try
            {
                count = DataBaseHelper.GetSharesFromDB().Count;
            }
            catch
            {
                return true;
            }

            // execute only when we have at least one share
            return count > 0;
        }
        #endregion

        #endregion
    }
}
