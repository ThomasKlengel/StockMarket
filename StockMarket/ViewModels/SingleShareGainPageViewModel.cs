using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for all <see cref="Order"/>s of a <see cref="Share"/>.
    /// </summary>
    class SingleShareGainPageViewModel : ShareComponentViewModel
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleShareGainPageViewModel"/> class.
        /// </summary>
        public SingleShareGainPageViewModel()
            : base()
        {
            this.Shares = new ObservableCollection<Share>();
        }

        private void DisplayedShare_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SumNow")
            {
                List<ShareComponentViewModel> all = new List<ShareComponentViewModel>();
                List<ShareComponentViewModel> divs = new List<ShareComponentViewModel>();

                foreach (var component in this.DisplayedShare.ShareComponents)
                {
                    all.Add(component);
                    if (component.ComponentType == ShareComponentType.Dividend)
                    {
                        divs.Add(component);
                    }
                }

                this.TileAll = new TileViewModel(all, ShareComponentType.Buy, "All");
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.TileAll)));
                this.TileDividends = new TileViewModel(divs, ShareComponentType.Dividend, "Dividends");
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.TileDividends)));
            }
        }

        /// <inheritdoc/>
        public override void UserChanged()
        {
            // gets all orders for the current user
            var ordersByUser = DataBaseHelper.GetAllItemsFromDB<Order>().Where(o => this.SelectByUser(o));
            // add any isin of these orders (HashSet only allows uniques -> duplicates are not added)
            var unique = new HashSet<string>();
            foreach (var order in ordersByUser)
            {
                unique.Add(order.ISIN);
            }

            // clear the colections
            this.Shares.Clear();
            var unsortedShares = new ObservableCollection<Share>();
            // get all shares for the user
            foreach (var isin in unique)
            {
                unsortedShares.Add(DataBaseHelper.GetItemsFromDB<Share>(isin).First());
            }

            // sort the shares by name
            foreach (var share in unsortedShares.OrderBy((s) => { return s.ShareName; }))
            {
                this.Shares.Add(share);
            }

            if (this.Shares.Count > 0)
            {
                // set the selected share for initially creating the view model
                this.SelectedShare = this.Shares.First();
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets all orders of the selected share.
        /// </summary>
        public ShareViewModel DisplayedShare { get; set; }

        /// <summary>
        /// Gets the <see cref="Share"/>s that are currently managed in the database.
        /// </summary>
        public ObservableCollection<Share> Shares { get; private set; }

        private Share _selectedShare;

        /// <summary>
        /// Gets or sets the <see cref="Share"/> that is currently selected.
        /// </summary>
        public Share SelectedShare
        {
            get
            {
                return this._selectedShare;
            }

            set
            {
                if (this._selectedShare != value)
                {
                    this._selectedShare = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.SelectedShare)));

                    if (this.SelectedShare != null)
                    {
                        this.DisplayedShare = new ShareViewModel(this.SelectedShare);
                        // notify Share of CurrentUser so values are refreshed
                        ApplicationService.Instance.EventAggregator.GetEvent<UserChangedEvent>().Publish(this.CurrentUser);
                        this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.DisplayedShare)));
                        this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.DisplayedCharts)));
                        this.DisplayedShare.PropertyChanged -= this.DisplayedShare_PropertyChanged;
                        this.DisplayedShare.PropertyChanged += this.DisplayedShare_PropertyChanged;
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
        
        public LiveCharts.SeriesCollection DisplayedCharts
        {
            get
            { 
                var charts = Charts.ChartCreator.CreateCharts(SelectedShare);
                var collection = new LiveCharts.SeriesCollection();
                collection.Add(charts.OrderSeries);
                collection.Add(charts.AbsoluteSeries);
                collection.Add(charts.GrowthSeries);
                return collection;           
            }
        }

        #endregion

    }
}
