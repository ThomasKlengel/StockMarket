﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for a single <see cref="Order"/>
    /// </summary>
    public class AddOrderViewModel : ViewModelBase
    {
        #region ctor
        public AddOrderViewModel()
        {
            AddOrderCommand = new RelayCommand(AddOrder, CanAddOrder);
            Shares = new ObservableCollection<Share>();
            foreach (var share in  DataBaseHelper.GetSharesFromDB() )
            {
                Shares.Add(share);
            }
            SelectedShare = Shares.First();
        }

        #endregion

        #region Properties

        public ObservableCollection<Share> Shares { get; set; }

        private double _actPrice;
        /// <summary>
        /// The current price of a single share
        /// </summary>
        public double ActPrice
        {
            get { return _actPrice; }
            set
            {
                _actPrice = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ActPrice)));
            }
        }

        private double _expenses =10.0;
        /// <summary>
        /// The current price of a single share
        /// </summary>
        public double Expenses
        {
            get { return _expenses; }
            set
            {
                _expenses = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Expenses)));
            }
        }

        private Share _selectedShare;
        public Share SelectedShare
        {
            get { return _selectedShare; }
            set
            {
                _selectedShare = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedShare)));

                RefreshPriceAsync();
            }
        }

        private int _amount;
        /// <summary>
        /// The amount of shares purchased
        /// </summary>
        public int Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Amount)));
            }
        }             

        private OrderType _orderType = OrderType.buy;
        /// <summary>
        /// The type of order
        /// </summary>
        public OrderType OrderType
        {
            get { return _orderType; }
            set
            {
                _orderType = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(OrderType)));
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(OrderIsBuy)));
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(OrderType)));
            }
        }

        public bool OrderIsBuy
        {
            get
            {

                return OrderType == OrderType.buy;
            }
            set
            {
                OrderType = OrderType == OrderType.sell ? OrderType.buy : OrderType.sell;
            }
        }
        public bool OrderIsSell
        {
            get
            {
                return OrderType == OrderType.sell;
            }
            set
            {
                OrderType = OrderType == OrderType.sell ? OrderType.buy : OrderType.sell;
            }
        }

        private DateTime _dateTime = DateTime.Today;
        public DateTime OrderDate
        {
            get { return _dateTime; }
            set { _dateTime = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(OrderDate)));
            }
        }       
        
        #endregion

        #region Methods
        private async void RefreshPriceAsync()
        {
            if (SelectedShare != null)
            {
                var content = await WebHelper.getWebContent(SelectedShare.WebSite);
                var price = RegexHelper.GetSharPrice(content);
                ActPrice = price;
            }
        }
        #endregion

        #region Commands
        public RelayCommand AddOrderCommand { get; private set; }

        private void AddOrder(object o)
        {
            // create a new order
            Order order = new Order();
            order.Amount = Amount;
            order.OrderExpenses = Expenses;
            order.OrderType = OrderType;
            order.SharePrice = ActPrice;
            order.Date = OrderDate;
            order.ISIN = SelectedShare.ISIN;

            // add the order to the matching share
            DataBaseHelper.AddOrderToDB(order);

            Amount = 0;
        }

        private bool CanAddOrder(object o)
        {
            return Amount > 0 ? true : false;
        }        
        #endregion
    }
}