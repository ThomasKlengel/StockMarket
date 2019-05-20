using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using StockMarket.DataModels;


namespace StockMarket.ViewModels
{
    public class ShareViewModel : ViewModelBase
    {
        #region ctors
        public ShareViewModel()
        {
            Orders = new ObservableCollection<OrderViewModel>();
            DayValues = new ObservableCollection<DayValueViewModel>();
        }

        #endregion

        #region Properties

        private string _shareName;

        public string ShareName
        {
            get { return _shareName; }
            set
            {
                _shareName = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ShareName)));
            }
        }

        private string _webSite;

        public string WebSite
        {
            get { return _webSite; }
            set
            {
                _webSite = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(WebSite)));
            }
        }

        private string _wkn;

        public string WKN
        {
            get { return _wkn; }
            set
            {
                _wkn = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(WKN)));
            }
        }

        private string _isin;

        public string ISIN
        {
            get { return _isin; }
            set
            {
                _isin = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ISIN)));
            }
        }

        private double _actPrice;

        public double ActualPrice
        {
            get { return _actPrice; }
            set
            {
                _actPrice = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ActualPrice)));
            }
        }



        private ObservableCollection<OrderViewModel> _orders;
        public ObservableCollection<OrderViewModel> Orders
        {
            get { return _orders; }
            set
            {
                _orders = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Orders)));
            }
        }

        private ObservableCollection<DayValueViewModel> _dayValues;
        public ObservableCollection<DayValueViewModel> DayValues
        {
            get { return _dayValues; }
            set
            {
                _dayValues = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(DayValues)));
            }
        }

        #endregion

        #region Methods

        public static ShareViewModel CreateFromShare(SharesDataModel model, Share share)
        {
            ShareViewModel vm_Share = new ShareViewModel();
            vm_Share.ISIN = share.ISIN;
            vm_Share.WKN = share.WKN;
            vm_Share.ShareName = share.ShareName;
            vm_Share.WebSite = share.WebSite;
            var orderCollection = new ObservableCollection<OrderViewModel>();

            var orders = from o in model.Orders
                         where o.ISIN == vm_Share.ISIN
                         select o;

            foreach (var order in orders)
            {
                orderCollection.Add(new OrderViewModel()
                {
                    Date = order.Date,
                    Amount = order.Amount,
                    OrderExpenses = order.OrderExpenses,
                    OrderType = order.OrderType,
                    SharePrice = order.SharePrice
                });
            }
            vm_Share.Orders = orderCollection;

            var valueCollection = new ObservableCollection<DayValueViewModel>();

            var values = from sv in model.ShareValues
                         where sv.ISIN == vm_Share.ISIN
                         select sv;

            foreach (var dayVal in values)
            {
                valueCollection.Add(new DayValueViewModel()
                {
                    Date = dayVal.Date,
                    Price = dayVal.Price
                });
            }
            vm_Share.Orders = orderCollection;

            vm_Share.DayValues = valueCollection;

            return vm_Share;

        }
        #endregion
    }



}
