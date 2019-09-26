using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for all <see cref="Shares"/> in the <see cref="Pages.SharesOverviewPage"/>
    /// </summary>
    class SharesGainViewModel : ShareComponentViewModel
    {

        #region Constructors
        public SharesGainViewModel() : base()
        {
            // create an empty collection
            Shares = new ObservableCollection<ShareViewModel>();
            SortCommand = new RelayCommand(SortShares);
        }

        public override void UserChanged()
        {
            #region set Shares for the user
            // get all orders of the current user
            var ordersbyUser = DataBaseHelper.GetAllItemsFromDB<Order>().Where(o => (SelectByUser(o)));
            // add the ISINs for these orders (HashSet only allows uniques -> duplicates are not added)
            var unique = new HashSet<string>();
            foreach (var order in ordersbyUser)
            {
                unique.Add(order.ISIN);
            }
            // clear the collection
            Shares.Clear();
            // get all shares for the current user
            var unsortedShares = new ObservableCollection<Share>();
            foreach (var isin in unique)
            {
                unsortedShares.Add(DataBaseHelper.GetSharesFromDB().Where((s) => { return s.ISIN == isin; }).First());
            }

            // add the shares from the database to the collection (oderd by the name)
            foreach (var share in unsortedShares.OrderBy((s) => { return s.ShareName; }))
            {
                ShareViewModel svm = new ShareViewModel(share);
                Shares.Add(svm);
                // add a handler for updating the ui in relevant cases
                svm.PropertyChanged += Share_RelevantPropertyChanged;
            }
            #endregion




            // notify Shares of CurrentUser so they refresh their values
            ApplicationService.Instance.EventAggregator.GetEvent<UserChangedEvent>().Publish(CurrentUser);
        }
        #endregion

        #region Eventhandler
        private void Share_RelevantPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SumNow":
                    {
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(SumNow)));
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Difference)));
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Percentage)));
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Background)));
                        break;
                    }
                case "SumBuy":
                    {
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(SumBuy)));
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Difference)));
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Percentage)));
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Background)));
                        break;
                    }
                case "Amount":
                    {
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Amount)));
                        break;
                    }
                case "AmountSold":
                    {
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(AmountSold)));
                        break;
                    }
                default: break;
            }

            if (e.PropertyName == "SumNow")
            {
                List<ShareComponentViewModel> all = new List<ShareComponentViewModel>();
                List<ShareComponentViewModel> divs = new List<ShareComponentViewModel>();
                foreach (var share in Shares)
                {
                    foreach (var component in share.ShareComponents)
                    {
                        all.Add(component);
                        if (component.ComponentType == ShareComponentType.dividend)
                        {
                            divs.Add(component);
                        }
                    }
                }
                TileAll = new TileViewModel(all, ShareComponentType.buy);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(TileAll)));
                TileDividends = new TileViewModel(divs,ShareComponentType.dividend);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(TileAll)));
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// All shares managed by this program
        /// </summary>
        public ObservableCollection<ShareViewModel> Shares { get; private set; }

        /// <summary>
        /// The summed up price for all orders on the date of purchase
        /// </summary>
        override public double SumBuy
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
        override public double SumNow
        {
            get
            {
                double now = 0;

                foreach (var share in Shares)
                {
                    now += share.SumNow;
                }
                return now;
            }
        }

        /// <summary>
        /// The amount of bought shares
        /// </summary>
        override public double Amount
        {
            get
            {
                double amount = 0;
                foreach (var share in Shares)
                {
                    amount += share.Amount;
                }

                return amount;
            }
            set { return; }
        }

        /// <summary>
        /// The amount of sold shares
        /// </summary>
        public override double AmountSold
        {
            get
            {
                double amount = 0;
                foreach (var share in Shares)
                {
                    amount += share.AmountSold;
                }

                return amount;
            }
        }

        
        public TileViewModel TileAll
        {
            get;set;
        }

        public TileViewModel TileDividends
        {
            get; set;
        }
        #endregion

        #region Commands

        /// <summary>
        /// The command to execute 
        /// </summary>
        /// <param name="o">should be a <see cref="GridViewColumnHeader"/> which has been clicked</param>
        private void SortShares (object o)
        {
            if (Shares.Count > 1)
            {
                // check if clicked item is a column header
                if (o.GetType() == typeof(GridViewColumnHeader))
                {
                    var header = o as GridViewColumnHeader;

                    var headerClicked = "";
                    // get the propery name to sort by
                    // if the binding is a binding...
                    if (header.Column.DisplayMemberBinding.GetType() == typeof(Binding))
                    { //... set the header to the bound path
                        var content = header.Column.DisplayMemberBinding as Binding;
                        headerClicked = content.Path.Path;
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
                        lastSortAscending = true;
                    }

                    // Sort the shares
                    Shares = SortCollection<ShareViewModel>(Shares, headerClicked, lastSortAscending);   

                    // set the last sorted by for next sort
                    lastSortedBy = headerClicked;

                }
            }
        }

        #endregion
    }

}
