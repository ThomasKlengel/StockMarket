using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for a single <see cref="Order"/>
    /// </summary>
    public class OrderViewModel : ViewModelBase
    {
        #region ctor
        public OrderViewModel()
        {
            AddOrderCommand = new RelayCommand(AddOrder, CanAddOrder);
        }

        #endregion

        #region Properties
        private double _sharePrice;
        /// <summary>
        /// The prices of a single share at the day of purchase
        /// </summary>
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

        private DateTime _date;
        /// <summary>
        /// The date of the purchase
        /// </summary>
        public DateTime Date
        {
            get { return _date.Date; }
            set
            {
                _date = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Date)));
            }
        }

        /// <summary>
        /// The summed up price of the shares at the day of purchase
        /// (getter only)
        /// </summary>
        public double SumBuy {get { return SharePrice * _amount; } }

        /// <summary>
        /// The current summed up price of the shares
        /// (getter only)
        /// </summary>
        public double SumNow
        {
             get { return ActPrice * _amount; } 
        }

        /// <summary>
        /// The color for the order determined by 
        /// a positive or negative development of share prices
        /// </summary>
        public SolidColorBrush Backgropund
        {
            get { return SumNow - SumBuy > 0 ? new SolidColorBrush(Color.FromRgb(222,255,209)) : new SolidColorBrush(Color.FromRgb(255, 127, 127)); }
        }

        /// <summary>
        /// The development of share prices in percent
        /// </summary>
        public double Percentage
        {
            get { return SumNow / SumBuy - 1.0; }
        }

        private double _orderExpenses;
        /// <summary>
        /// The fees of the order
        /// </summary>
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
            }
        }

        #endregion

        #region Commands
        public RelayCommand AddOrderCommand { get; private set; }

        private void AddOrder(object o)
        {
            if (o != null)
            {
                if (o.GetType() == typeof(Button))
                {
                    Button b = o as Button;
                    var sp = b.Parent as StackPanel;
                    ComboBox cobo=null;
                    foreach (var child in sp.Children)
                    {
                        if (child.GetType()==typeof(ComboBox))
                        {
                            cobo = child as ComboBox;
                            break;
                        }
                    }
                    
                    // create a new order
                    Order order = new Order();
                    order.Amount = Convert.ToInt32(Amount);
                    order.OrderExpenses = 10;
                    order.OrderType = OrderType.buy;
                    order.SharePrice = Convert.ToDouble(SharePrice);
                    order.Date = DateTime.Today;
                    order.ISIN = (cobo.SelectedItem as ShareViewModel).ISIN;

                    // add the order to the matching share
                    DataBaseHelper.AddOrderToDB(order);
                }
            }

        }

        private bool CanAddOrder (object o)
        {
            return Amount > 0 ? true : false;
        }
        #endregion
    }
}
