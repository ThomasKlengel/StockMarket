using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for all <see cref="Shares"/> in the <see cref="Pages.SharesOverviewPage"/>.
    /// </summary>
    class SharesGainViewModel : ShareComponentViewModel
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SharesGainViewModel"/> class.
        /// </summary>
        public SharesGainViewModel()
            : base()
        {
            // create an empty collection
            this.Shares = new ObservableCollection<ShareViewModel>();
            this.SortCommand = new RelayCommand(this.SortShares);
        }

        /// <inheritdoc/>
        public override void UserChanged()
        {
            #region set Shares for the user
            // get all orders of the current user
            var ordersbyUser = DataBaseHelper.GetAllItemsFromDB<Order>().Where(o => this.SelectByUser(o));
            // add the ISINs for these orders (HashSet only allows uniques -> duplicates are not added)
            var unique = new HashSet<string>();
            foreach (var order in ordersbyUser)
            {
                unique.Add(order.ISIN);
            }

            // clear the collection
            this.Shares.Clear();
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
                this.Shares.Add(svm);
                // add a handler for updating the ui in relevant cases
                svm.PropertyChanged += this.Share_RelevantPropertyChanged;
            }
            #endregion

            // notify Shares of CurrentUser so they refresh their values
            ApplicationService.Instance.EventAggregator.GetEvent<UserChangedEvent>().Publish(this.CurrentUser);
        }
        #endregion

        #region Eventhandler
        private void Share_RelevantPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SumNow")
            {
                // change the Sum Tiles displayed values
                List<ShareComponentViewModel> all = new List<ShareComponentViewModel>();
                List<ShareComponentViewModel> current = new List<ShareComponentViewModel>();
                List<ShareComponentViewModel> divs = new List<ShareComponentViewModel>();
                // go through each share (of the current user)
                foreach (var share in this.Shares)
                {
                    double currentAmount = 0;
                    // calc the current amount of shares
                    foreach (var component in share.ShareComponents)
                    {
                        if (component.ComponentType == ShareComponentType.Buy)
                        {
                            currentAmount += component.Amount;
                            currentAmount -= component.AmountSold;
                        }
                    }

                    foreach (var component in share.ShareComponents)
                    {   // add all ccomponents to the "all" tile
                        all.Add(component);
                        // add dividends to the "dividends" tile
                        if (component.ComponentType == ShareComponentType.Dividend)
                        {
                            divs.Add(component);
                        }

                        // add shares where there is at least one share is currently held to the "current" tile
                        if (currentAmount > 0 && component.ComponentType == ShareComponentType.Buy)
                        {
                            current.Add(component);
                        }
                    }
                }

                // update the view of the tiles
                this.TileAll = new TileViewModel(all, ShareComponentType.Buy, "All");
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.TileAll)));
                this.TileDividends = new TileViewModel(divs,ShareComponentType.Dividend, "Dividends");
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.TileDividends)));
                this.TileCurrent = new TileViewModel(current, ShareComponentType.Buy, "Current");
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.TileCurrent)));
            }
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets all shares managed by this program.
        /// </summary>
        public ObservableCollection<ShareViewModel> Shares { get; private set; }

        public TileViewModel TileAll
        {
            get;set;
        }

        public TileViewModel TileDividends
        {
            get; set;
        }

        public TileViewModel TileCurrent
        {
            get; set;
        }
        #endregion

        #region Commands

        /// <summary>
        /// The command to execute.
        /// </summary>
        /// <param name="o">should be a <see cref="GridViewColumnHeader"/> which has been clicked.</param>
        private void SortShares (object o)
        {
            if (this.Shares.Count > 1)
            {
                // check if clicked item is a column header
                if (o.GetType() == typeof(GridViewColumnHeader))
                {
                    var header = o as GridViewColumnHeader;

                    var headerClicked = string.Empty;
                    // get the propery name to sort by
                    // if the binding is a binding...
                    if (header.Column.DisplayMemberBinding.GetType() == typeof(Binding))
                    { // ... set the header to the bound path
                        var content = header.Column.DisplayMemberBinding as Binding;
                        headerClicked = content.Path.Path;
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
                        this.lastSortAscending = true;
                    }

                    // Sort the shares
                    this.Shares = this.SortCollection<ShareViewModel>(this.Shares, headerClicked, this.lastSortAscending);

                    // set the last sorted by for next sort
                    this.lastSortedBy = headerClicked;
                }
            }
        }

        #endregion
    }
}
