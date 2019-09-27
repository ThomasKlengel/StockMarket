using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for all <see cref="Order"/>s of a <see cref="Share"/>
    /// </summary>
    class SingleShareGainPageViewModel : ShareComponentViewModel
    {

        #region Constructors
        public SingleShareGainPageViewModel():base()
        {
            Shares = new ObservableCollection<Share>();
        }

        private void DisplayedShare_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SumNow")
            {
                List<ShareComponentViewModel> all = new List<ShareComponentViewModel>();
                List<ShareComponentViewModel> divs = new List<ShareComponentViewModel>();

                foreach (var component in DisplayedShare.ShareComponents)
                {
                    all.Add(component);
                    if (component.ComponentType == ShareComponentType.dividend)
                    {
                        divs.Add(component);
                    }
                }

                TileAll = new TileViewModel(all, ShareComponentType.buy, "All");
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(TileAll)));
                TileDividends = new TileViewModel(divs, ShareComponentType.dividend, "Dividends");
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(TileDividends)));
            }
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
            }

        }

        #endregion

        #region Properties           

        /// <summary>
        /// All orders of the selected share
        /// </summary>
        public ShareViewModel DisplayedShare { get; set; }

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
                        DisplayedShare = new ShareViewModel(SelectedShare);
                        // notify Share of CurrentUser so values are refreshed
                        ApplicationService.Instance.EventAggregator.GetEvent<UserChangedEvent>().Publish(CurrentUser);
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(DisplayedShare)));
                        DisplayedShare.PropertyChanged -= DisplayedShare_PropertyChanged;
                        DisplayedShare.PropertyChanged += DisplayedShare_PropertyChanged;


                    }
                }
            }
        }

        public TileViewModel TileAll
        {
            get; set;
        }

        public TileViewModel TileDividends
        {
            get; set;
        }

        #endregion

    }
}
