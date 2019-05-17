using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace StockMarket.ViewModels
{
    public class OrderViewModel : ViewModelBase
    {
        #region Properties
        private double _orderPrice;

        public double SharePrice
        {
            get { return _orderPrice; }
            set
            {
                _orderPrice = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SharePrice)));
            }
        }

        private int _amount;

        public int Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Amount)));
            }
        }

        private DateTime _date;

        public DateTime Date
        {
            get { return _date.Date; }
            set
            {
                _date = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Date)));
            }
        }

        private double _orderExpenses;

        public double OrderExpenses
        {
            get { return _orderExpenses; }
            set
            {
                _orderExpenses = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(OrderExpenses)));
            }
        }

        private OrderType _orderType;

        public OrderType OrderType
        {
            get { return _orderType; }
            set
            {
                _orderType = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(OrderType)));
            }
        }

        #endregion

    }
}
