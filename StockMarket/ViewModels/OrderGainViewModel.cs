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
    /// A ViewModel for all <see cref="Order"/>s of a <see cref="Share"/>
    /// </summary>
    class OrderGainViewModel : ShareComponentViewModel
    {

        //TODO: rename ViewModel, since its essentially the VieModel for a "ShareGainViewModel"
        //create an additional one for the page it is displayed on, inheriting from this one


        #region Constructors
        public OrderGainViewModel():base()
        {
            Shares = new ObservableCollection<Share>();
            ShareComponents = new ObservableCollection<ShareComponentViewModel>();
        }

        public override void UserChanged()
        {
            // gets all orders for the current user
            var ordersByUser = DataBaseHelper.GetAllItemsFromDB<Order>().Where(o=>(SelectByUser(o)));
            // add any isin of these orders (HashSet only allows uniques -> duplicates are not added)
            var unique = new HashSet<string>();
            foreach (var order in ordersByUser)
            {
                unique.Add(order.ISIN);
            }
            // clear the colections
            Shares.Clear();
            ShareComponents.Clear();
            var unsortedShares = new ObservableCollection<Share>();
            // get all shares for the user
            foreach (var isin in unique)
            {
                unsortedShares.Add(DataBaseHelper.GetItemsFromDB<Share>(isin).First());
            }
            // sort the shares by name
            foreach (var share in unsortedShares.OrderBy((s) => { return s.ShareName; }))
            {
                Shares.Add(share);
            }

            if (Shares.Count > 0)
            {
                // set the selected share for initially creating the view model
                SelectedShare = Shares.First();

                // set the command for sorting the Orders
                SortCommand = new RelayCommand(SortOrders);

                // create a timer for refreshing the shown prices
                var refrehTimer = new DispatcherTimer();
                refrehTimer.Interval = new TimeSpan(0, 10, 0);
                refrehTimer.Tick += RefrehTimer_Tick;
                refrehTimer.Start();
            }
        }

        #endregion

        #region Properties
        /// <summary>
        /// The average share price for the orders
        /// </summary>
        public double AvgSharePrice
        {
            get
            {
                double sum = 0;
                foreach (var order in ShareComponents)
                {
                    sum += order.SinglePriceBuy;
                };
                return sum / ShareComponents.Count;
            }
        }


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

        /// <summary>
        /// The <see cref="Share"/>s that are currently managed in the database
        /// </summary>
        public ObservableCollection<Share> Shares { get; private set; }

        private Share _selectedShare;
        /// <summary>
        /// The <see cref="Share"/> that is currently selected
        /// </summary>
        public Share SelectedShare
        {
            get { return _selectedShare; }
            set
            {
                if (_selectedShare != value)
                {
                    _selectedShare = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedShare)));

                    if (SelectedShare != null)
                    {
                        // refresh the orders list
                        SetOrdersInitially();
                        // refresh the prices for the orders
                        RefreshPriceAsync();
                    }
                }
            }
        }

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
            foreach (var order in DataBaseHelper.GetItemsFromDB<Order>(SelectedShare)
                .Where(o => SelectByUser(o)))
            {
                ShareComponents.Add(new OrderViewModel(order));
            }

            // add the dividends from the database for this user
            foreach (var dividend in DataBaseHelper.GetItemsFromDB<Dividend>(SelectedShare)
                  .Where(dividend => SelectByUser(dividend)))
            {
                ShareComponents.Add(new DividendViewModel(dividend));
            }

            //sort the displayed by Date
            ShareComponents = SortCollection(ShareComponents, "Date", false);

            // notify UI of changes
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Amount)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(AmountSold)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(AvgSharePrice)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(SumBuy)));

        }

        /// <summary>
        /// refreshes the actual <see cref="Share"/> price
        /// </summary>
        private async void RefreshPriceAsync()
        {
            // get the website content
            var content = await WebHelper.getWebContent(SelectedShare.WebSite);
            //get the price
            var price = RegexHelper.GetSharePrice(content, SelectedShare.ShareType);
            if (price == 0.0)
            {
                 content = await WebHelper.getWebContent(SelectedShare.WebSite2);
                //get the price
                 price = RegexHelper.GetSharePrice(content, SelectedShare.ShareType);
            }
            if (price == 0.0)
            {
                content = await WebHelper.getWebContent(SelectedShare.WebSite3);
                //get the price
                price = RegexHelper.GetSharePrice(content, SelectedShare.ShareType);
            }

            //set the price for the UI
            SinglePriceNow = price;
        }
        #endregion

        #region Handler

        /// <summary>
        /// Eventhandler that refreshes the current price
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefrehTimer_Tick(object sender, EventArgs e)
        {
            //refresh the actual prices
            RefreshPriceAsync();
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
