using System;
using System.ComponentModel;
using System.Windows.Media;

namespace StockMarket.ViewModels
{
    public class OrderViewModel : ViewModelBase
    {
        #region Properties
        private double _sharePrice;

        public double SharePrice
        {
            get { return _sharePrice; }
            set
            {
                _sharePrice = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SharePrice)));
            }
        }

        private double _actPrice;

        public double ActPrice
        {
            get { return _actPrice; }
            set
            {
                _actPrice = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ActPrice)));
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

        public double SumBuy {get { return SharePrice * _amount; } }

        public double SumNow
        {
             get { return ActPrice * _amount; } 
        }

        public SolidColorBrush Backgropund
        {
            get { return SumNow - SumBuy > 0 ? new SolidColorBrush(Color.FromRgb(222,255,209)) : new SolidColorBrush(Color.FromRgb(255, 127, 127)); }
        }

        public double Percentage
        {
            get { return SumNow / SumBuy - 1.0; }
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
