using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace StockMarket.ViewModels
{
    class MainWindowViewModel: ViewModels.ViewModelBase
    {               

        public MainWindowViewModel()
        {
            DisplayPage = new Pages.BlankPage();
            AddShareCommand = new RelayCommand(AddShare);
            AddOrderCommand = new RelayCommand(AddOrder,CanAddOrder);
            DisplayOrderOverviewCommand = new RelayCommand(DisplayOrderOverview, CanDisplayOrderOverview);
            DisplayShareOverviewCommand = new RelayCommand(DisplayShareOverview, CanDisplayShareOverview);
        }

        #region Properties
        private Page _displayPage;
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

        public RelayCommand AddShareCommand { get; private set; }
        public RelayCommand AddOrderCommand { get; private set; }
        public RelayCommand DisplayOrderOverviewCommand { get; private set; }
        public RelayCommand DisplayShareOverviewCommand { get; private set; }

        private void AddShare(object o)
        {
            DisplayPage = new Pages.AddSharePage();
        }

        private void AddOrder(object o)
        {            
            DisplayPage = new Pages.AddOrderPage();
        }

        private bool CanAddOrder (object o)
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
            return shares > 0 ;
        }

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
            return orders>0;
        }

        private void DisplayShareOverview(object o)
        {
            DisplayPage = new Pages.SharesOverviewPage();
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
    }
}
