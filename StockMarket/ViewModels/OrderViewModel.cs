using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.Collections.Generic;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for a single <see cref="Order"/>
    /// </summary>
    public class OrderViewModel : ViewModelBase
    {
        #region ctor
        /// <summary>
        /// Creates a ViewModel for an order with an AddOrderCommand 
        /// </summary>
        public OrderViewModel()
        {
            AddOrderCommand = new RelayCommand(AddOrder, CanAddOrder);
        }

        /// <summary>
        /// Creates a ViewModel for an order from an order 
        /// </summary>
        /// <param name="order">The <see cref="Order"/> to create a ViewModel for</param>
        public OrderViewModel(Order order)
        {
            this.Amount = order.Amount;
            this.Date = order.Date;
            this.OrderExpenses = order.OrderExpenses;
            this.OrderType = order.OrderType;
            this.SharePrice = order.SharePrice;
            this.ISIN = order.ISIN;          
        }

        #endregion

        #region Properties
        private string ISIN;

        private double _sharePrice;
        /// <summary>
        /// The prices of a single share at the day of purchase
        /// </summary>
        public double SharePrice
        {
            get { return _sharePrice; }
            set
            {
                if (_sharePrice != value)
                {
                    _sharePrice = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SharePrice)));
                }
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
                if (_actPrice != value)
                {
                    _actPrice = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ActPrice)));
                    if (OrderType == OrderType.buy)
                    {
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(SumNow)));
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Percentage)));
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Backgropund)));
                    }
                }
            }
        }


        private int _amount;
        /// <summary>
        /// The amount of shares purchased
        /// </summary>
        public int Amount
        {
            get { return _amount*(int)OrderType; }
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Amount)));
                }
            }
        }

        public int AmountSold
        {
            get
            {
                switch (OrderType)
                {
                    case OrderType.buy:
                        {
                            //get buys and sells
                            var orders = DataBaseHelper.GetOrdersFromDB(ISIN);
                            var sells = orders.FindAll(o => o.OrderType == OrderType.sell).OrderBy(o => o.Date).ToList();
                            var buys = orders.FindAll(o => o.OrderType == OrderType.buy && o.Date < Date).OrderBy(o => o.Date).ToList();

                            //sum up the amount of sold shares
                            int numSells = 0;
                            foreach (var order in sells)
                            {
                                numSells += order.Amount;
                            }

                            // remove them from the buys prior to this one
                            for (int i = 0; i < buys.Count(); i++)
                            {
                                if (numSells < 1)
                                {
                                    break;
                                }

                                if (numSells > buys[i].Amount)
                                {
                                    numSells -= buys[i].Amount;
                                    buys[i].Amount = 0;
                                }
                                else
                                {
                                    buys[i].Amount -= numSells;
                                    numSells = 0;
                                }
                            }
                            return _amount < numSells ? _amount : numSells;
                        }
                    default: return 0;
                }
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
                if (_date != value)
                {
                    _date = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Date)));
                }
            }
        }

        /// <summary>
        /// The summed up price of the shares at the day of purchase
        /// (getter only)
        /// </summary>
        public double SumBuy
        {
            get
            {
                switch (OrderType)
                {
                    case OrderType.sell:
                        {
                            //get buys and sells
                            var orders = DataBaseHelper.GetOrdersFromDB(ISIN);
                            var sells = orders.FindAll(o => o.OrderType == OrderType.sell && o.Date<Date).OrderBy(o => o.Date).ToList();
                            var buys = orders.FindAll(o => o.OrderType == OrderType.buy).OrderBy(o => o.Date).ToList();

                            //sum up the amount of sold shares prior to this one
                            int numSells = 0;
                            foreach (var order in sells)
                            {
                                numSells += order.Amount;
                            }
                            
                            // remove the sold ones from the buys
                            for (int i = 0; i < buys.Count(); i++)
                            {
                                if (numSells < 1) // break when sells are 0
                                {
                                    break;
                                }

                                if (numSells > buys[i].Amount) // if remaining sells are greater then the amount of shares in the actual buy order
                                {
                                    // ...remove Amount from sells, set Amount to 0 the actual buy order
                                    numSells -= buys[i].Amount;
                                    buys[i].Amount = 0;
                                }
                                else// the amount of shares bought with this order is greater then the remaining sells, so...
                                {
                                    // ... remove the remaining sells from the actual buy order, set numSells to 0  
                                    buys[i].Amount -= numSells;
                                    numSells = 0;
                                }                                
                            }

                            // sum up the prices
                            double buyprice = 0.0;
                            int tempamount = _amount;
                            for (int i = 0; i < buys.Count(); i++)
                            {
                                if (tempamount==0) // break when the amount remaining for this sell order is 0
                                {
                                    break;
                                }
                                if (buys[i].Amount==0) // skip if the actual buy has no more shares
                                {
                                    continue;
                                }

                                if (tempamount > buys[i].Amount) // when the shares remaining in this sell order is greater then the shares bought...
                                {
                                    // ...remove the boughht shares from the remaining shares
                                    tempamount -= buys[i].Amount;
                                    // add the buyprice of the buy order, take into account the whole order expenses
                                    buyprice += buys[i].Amount * buys[i].SharePrice + buys[i].OrderExpenses;
                                }
                                else // when the shares remaining in this sell order are smaller then the shares bought...
                                {
                                    // add the proportion of the buyprice of the buy order, take into account the proportional order expenses
                                    buyprice += tempamount * buys[i].SharePrice + buys[i].OrderExpenses * tempamount / buys[i].Amount;

                                    tempamount = 0;
                                    break;
                                }
                            }
                            return buyprice;
                        }                        
                    default: // buys, for safety as default
                        {                        
                            // sum up the prices                     
                            return SharePrice * (_amount - AmountSold);
                        }
                }                
            }
        }

        /// <summary>
        /// The current summed up price of the shares
        /// (getter only)
        /// </summary>
        public double SumNow
        {
            get
            {
                switch (OrderType)
                {
                    case OrderType.sell: return SharePrice * _amount + OrderExpenses;
                    default: // buys, default for safety
                        {
                            //get buys and sells
                            var orders = DataBaseHelper.GetOrdersFromDB(ISIN);
                            var sells = orders.FindAll(o => o.OrderType == OrderType.sell).OrderBy(o => o.Date).ToList();
                            var buys = orders.FindAll(o => o.OrderType == OrderType.buy && o.Date < Date).OrderBy(o => o.Date).ToList();

                            //sum up the amount of sold shares
                            int numSells = 0;
                            foreach (var order in sells)
                            {
                                numSells += order.Amount;
                            }

                            // remove the sold shares from the buys prior to this one
                            for (int i = 0; i < buys.Count(); i++)
                            {
                                if (numSells < 1) // break if sells are 0
                                {
                                    break;
                                }

                                if (numSells > buys[i].Amount) // if the remaining sells are greater then the amount of shares in the buy order...
                                {
                                    // ...remove Amount from sells, set Amount to 0 the actual buy order
                                    numSells -= buys[i].Amount;
                                    buys[i].Amount = 0;
                                }
                                else // the amount of shares bought with this order is greater then the remaining sells, so...
                                {
                                    // ... remove the remaining sells from the actual buy order, set numSells to 0  
                                    buys[i].Amount -= numSells;
                                    numSells = 0;
                                }
                            }

                            // sum up the prices, take into account the sold ones and probable order expenses                     
                            return ActPrice * (_amount - numSells) + (numSells < _amount ? OrderExpenses * ((_amount - numSells) / _amount) : 0);
                        }
                }
            }
        }

        /// <summary>
        /// The color for the order determined by 
        /// a positive or negative development of share prices
        /// </summary>
        public Brush Backgropund
        {
            get
            {
                var paleRed = Color.FromRgb(255, 127, 127);
                var paleGreen = Color.FromRgb(222, 255, 209);
                var color = Percentage >= 0 ? paleGreen : paleRed;
                Brush solidBack = new SolidColorBrush(color);
                Brush gradientBack = new LinearGradientBrush(Colors.Gray, color, 0);

                return Amount-AmountSold > 0 ? solidBack : gradientBack;
            }
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
                if (_orderExpenses != value)
                {
                    _orderExpenses = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(OrderExpenses)));
                }
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
                if (_orderType != value)
                {
                    _orderType = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(OrderType)));
                }
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
