using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for all <see cref="Order"/>s of a <see cref="Share"/>
    /// </summary>
    class OrderOverviewViewModel : CollectionViewModel
    {
        #region ctor
        public OrderOverviewViewModel()
        {
            Shares = DataBaseHelper.GetSharesFromDB();
            SelectedShare = Shares.First();

            var refrehTimer = new DispatcherTimer();
            refrehTimer.Interval = new TimeSpan(0, 10, 0);
            refrehTimer.Tick += RefrehTimer_Tick;
            refrehTimer.Start();
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
                foreach (var order in Orders)
                {
                    sum += order.SharePrice;
                };
                return sum / Orders.Count;
            }
        }

        private double _actPrice;

        /// <summary>
        /// The current share price for the orders
        /// </summary>
        public double ActPrice
        {
            get { return _actPrice; }
            set
            {
                if (_actPrice != value)
                {
                    _actPrice = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ActPrice)));

                    foreach (var order in Orders)
                    {
                        order.ActPrice = (ActPrice);
                    }

                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SumNow)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Percentage)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Background)));
                }
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
                foreach (var order in Orders)
                {
                    if (order.OrderType == OrderType.buy)
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
                foreach (var order in Orders)
                {
                    if (order.OrderType == OrderType.sell)
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

                foreach (var order in Orders)
                {
                    if (order.OrderType == OrderType.buy)
                    {
                        sum += order.SumBuy;
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
                foreach (var order in Orders)
                {
                    if (order.OrderType == OrderType.buy)
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
        public ObservableCollection<OrderViewModel> Orders { get; set; }

        /// <summary>
        /// The <see cref="Share"/>s that are currently managed in the database
        /// </summary>
        public List<Share> Shares { get; private set; }

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

                    // refresh the orders list
                    selectOrders();
                    // refresh the prices for the orders
                    RefreshPriceAsync();
                }
            }
        }

        /// <summary>
        /// Selects the <see cref="Orders"/> associated to the selected <see cref="Share"/>
        /// and add them to the <see cref="Orders"/> property
        /// </summary>
        private void selectOrders()
        {
            // get the orders from the databse
            var sortedOrders = DataBaseHelper.GetOrdersFromDB(SelectedShare.ISIN).OrderByDescending((o) => { return o.Date; });
            
            // create or clear the list of Orders
            if (Orders==null)
            {
                Orders = new ObservableCollection<OrderViewModel>();
            }
            Orders.Clear();

            // add the orders from the database
            foreach (var order in sortedOrders)
            {
                Orders.Add(new OrderViewModel(order));                    
            }

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
            var price=  RegexHelper.GetSharePrice(content,SelectedShare.ShareType);
            //set the price for the UI
            ActPrice = price;
        }

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
    }
}
