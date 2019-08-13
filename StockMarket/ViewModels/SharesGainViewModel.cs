using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for all <see cref="Shares"/> in the <see cref="Pages.SharesOverviewPage"/>
    /// </summary>
    class SharesGainViewModel : CollectionViewModel
    {
        #region Constructors
        public SharesGainViewModel()
        {
            // create an empty collection
            Shares = new ObservableCollection<ShareGainViewModel>();
            // get the all shares from the database
            var shares = DataBaseHelper.GetSharesFromDB();

            // add the shares from the database to the collection
            foreach (var share in shares)
            {
                ShareGainViewModel svm = new ShareGainViewModel(share);
                Shares.Add(svm);
                // add a handler for updating the ui in relevant cases
                svm.PropertyChanged += Share_RelevantPropertyChanged;
            }

            // create a command for sorting the shares
            SortCommand = new RelayCommand(SortShares);
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
                default: break;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// All shares managed by this program
        /// </summary>
        public ObservableCollection<ShareGainViewModel> Shares { get; private set; }

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
        override public int Amount
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
            set { return; }
        }

        /// <summary>
        /// The amount of sold shares
        /// </summary>
        public override int AmountSold
        {
            get
            {
                int amount = 0;
                foreach (var share in Shares)
                {
                    amount += share.AmountSold;
                }

                return amount;
            }
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
                    Shares = SortCollection<ShareGainViewModel>(Shares, headerClicked, lastSortAscending);   

                    // set the last sorted by for next sort
                    lastSortedBy = headerClicked;

                }
            }
        }

        #endregion
    }

}
