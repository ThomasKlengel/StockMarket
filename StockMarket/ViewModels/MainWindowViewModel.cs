using System.ComponentModel;
using System.Windows.Controls;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// a view model for the main window
    /// </summary>
    class MainWindowViewModel: ViewModels.ViewModelBase
    {          
        /// <summary>
        /// creates a new instance of a ViewModel for the MainWindow
        /// </summary>
        public MainWindowViewModel()
        {
            // set the start page to an empty page
            DisplayPage = new Pages.BlankPage();

            // define the commands for the buttons
            AddShareCommand = new RelayCommand(AddShare);
            AddOrderCommand = new RelayCommand(AddOrder,CanAddOrder);
            DisplayOrderOverviewCommand = new RelayCommand(DisplayOrderOverview, CanDisplayOrderOverview);
            DisplayShareOverviewCommand = new RelayCommand(DisplayShareOverview, CanDisplayShareOverview);
        }

        #region Properties
        private Page _displayPage;
        /// <summary>
        /// The <see cref="Page"/> to display in the main frame
        /// </summary>
        public Page DisplayPage {
            get
            {
                return _displayPage;
            }
            private set
            {
                _displayPage = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(DisplayPage)));
            }
                }
        #endregion

        #region Commands             

        #region Add Share Command        
        public RelayCommand AddShareCommand { get; private set; }

        private void AddShare(object o)
        {
            DisplayPage = new Pages.AddSharePage();
        }
        #endregion

        #region Add Order Command
        public RelayCommand AddOrderCommand { get; private set; }

        private void AddOrder(object o)
        {
            DisplayPage = new Pages.AddOrderPage();
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

        #region Display Order Overview Command
        public RelayCommand DisplayOrderOverviewCommand { get; private set; }

        private void DisplayOrderOverview(object o)
        {
            DisplayPage = new Pages.OrdersOverviewPage();
        }

        private bool CanDisplayOrderOverview(object o)
        {
            int orders = 0;

            // try-catch only for UI generator
            try
            {
                var shares = DataBaseHelper.GetSharesFromDB();
                foreach (var share in shares)
                {
                    orders += DataBaseHelper.GetOrdersFromDB(share).Count;
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

        #region Display Share Overview Command
        public RelayCommand DisplayShareOverviewCommand { get; private set; }

        private void DisplayShareOverview(object o)
        {
            DisplayPage = new Pages.SharesOverviewPage2();
        }

        private bool CanDisplayShareOverview(object o)
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
