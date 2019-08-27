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
    public class OrderViewModel : CollectionViewModel
    {
        #region ctor
        /// <summary>
        /// Creates a ViewModel for an order with an AddOrderCommand 
        /// </summary>
        public OrderViewModel() : base()
        {
            AddOrderCommand = new RelayCommand(AddOrder, CanAddOrder);
        }

        /// <summary>
        /// Creates a ViewModel for an order from an order 
        /// </summary>
        /// <param name="order">The <see cref="Order"/> to create a ViewModel for</param>
        public OrderViewModel(Order order) : base()
        {
            this.Amount = order.Amount;
            this.BookingDate = order.Date;
            this.OrderExpenses = order.OrderExpenses;
            this.ComponentType = order.OrderType;
            this.SinglePriceBuy = order.SharePrice;
            this.ISIN = order.ISIN;
            this.UserName = order.UserName;
        }

        #endregion

        #region Properties

        public string UserName { get; private set; }

        private readonly string ISIN;


        override public int AmountSold
        {
            get
            {
                switch (ComponentType)
                {
                    case ShareComponentType.buy:
                        {
                            //get buys and sells
                            var orders = DataBaseHelper.GetItemsFromDB<Order>(ISIN);
                            var sells = orders.FindAll(o => o.OrderType == ShareComponentType.sell).OrderBy(o => o.Date).ToList();
                            var buysPrior = orders.FindAll(o => o.OrderType == ShareComponentType.buy && o.Date < BookingDate).OrderBy(o => o.Date).ToList();

                            //sum up the amount of sold shares
                            int numSells = 0;
                            foreach (var order in sells)
                            {
                                numSells += order.Amount;
                            }

                            // remove them from the buys prior to this one
                            for (int i = 0; i < buysPrior.Count(); i++)
                            {
                                if (numSells < 1)
                                {
                                    break;
                                }

                                if (numSells > buysPrior[i].Amount)
                                {
                                    numSells -= buysPrior[i].Amount;
                                    buysPrior[i].Amount = 0;
                                }
                                else
                                {
                                    buysPrior[i].Amount -= numSells;
                                    numSells = 0;
                                }
                            }
                            return Amount < numSells ? Amount : numSells;
                        }
                    default: return Amount;
                }
            }
        }

        /// <summary>
        /// The summed up price of the shares at the day of purchase
        /// (getter only)
        /// </summary>
        override public double SumBuy
        {
            get
            {
                switch (ComponentType)
                {
                    case ShareComponentType.sell:
                        {
                            //get buys and sells
                            var orders = DataBaseHelper.GetItemsFromDB<Order>(ISIN);
                            var sellsPrior = orders.FindAll(o => o.OrderType == ShareComponentType.sell && o.Date < BookingDate).OrderBy(o => o.Date).ToList();
                            var buys = orders.FindAll(o => o.OrderType == ShareComponentType.buy).OrderBy(o => o.Date).ToList();

                            List<int> soldRemaining = new List<int>();
                            List<int> buysRemaining = new List<int>();
                            //put the amounts of sold/bought shares into a List
                            foreach (var order in sellsPrior)
                            {
                                soldRemaining.Add(order.Amount);
                            }
                            foreach (var order in buys)
                            {
                                buysRemaining.Add(order.Amount);
                            }

                            //leave only the sells that are not already sold
                            for (int i = 0; i < buysRemaining.Count; i++)
                            {
                                for (int j = 0; j < soldRemaining.Count; j++)
                                {
                                    if (buysRemaining[i] <= 0)
                                    {
                                        break;
                                    }
                                    if (soldRemaining[j] <= 0)
                                    {
                                        continue;
                                    }

                                    if (buysRemaining[i] <= soldRemaining[j])
                                    {
                                        soldRemaining[j] -= buysRemaining[i];
                                        buysRemaining[i] = 0;
                                    }
                                    else
                                    {
                                        buysRemaining[i] -= soldRemaining[j];
                                        soldRemaining[j] = 0;
                                    }
                                }
                            }

                            double sum = 0.0;
                            int tempAmount = Amount;
                            // go through all sells and add up the prices
                            for (int i = 0; i < buysRemaining.Count; i++)
                            {
                                if (tempAmount <= 0) // if amount remaining for this share is 0...
                                {
                                    break; // ... we can stop the loop
                                }
                                if (buysRemaining[i] <= 0) // if the the amount for the sell is 0...
                                {
                                    continue; // ... go to the next sell
                                }

                                if (buysRemaining[i] >= tempAmount) // if the sells are more or equal the remaining amount...
                                {
                                    sum += tempAmount * buys[i].SharePrice; // ... add up the price for the remaining amount (the sharepirce of the sell)
                                    sum += buys[i].OrderExpenses * (tempAmount / buys[i].Amount); // ...remove the (partial) order expenses
                                                                                                  // set the remaining amounts
                                    buysRemaining[i] -= tempAmount;
                                    tempAmount = 0;
                                }
                                else // if the remaining amount is larger...
                                {
                                    sum += buysRemaining[i] * buys[i].SharePrice; // ... add up the price for the sold amount (using the sharepirce of the sell)
                                    sum += buys[i].OrderExpenses * (buysRemaining[i] / buys[i].Amount); // ...remove the (partial) order expenses
                                                                                                        // set the remaining amounts
                                    tempAmount -= buysRemaining[i];
                                    buysRemaining[i] = 0;
                                }
                            }

                            // app up price for the remaining (not sold) shares (using the current price)
                            sum += tempAmount * SinglePriceNow;
                            sum += OrderExpenses * (tempAmount / Amount); // remove the (patial) order expenses

                            return sum;
                        }
                    default: // buys, for safety as default
                        {
                            // sum up the prices                     
                            return SinglePriceBuy * (Amount) + OrderExpenses;
                        }
                }
            }
        }

        /// <summary>
        /// The current summed up price of the shares
        /// (getter only)
        /// </summary>
        override public double SumNow
        {
            get
            {
                switch (ComponentType)
                {
                    case ShareComponentType.sell: return SinglePriceBuy * AmountSold + OrderExpenses;
                    default: // buys, default for safety
                        {
                            //get buys and sells
                            var orders = DataBaseHelper.GetItemsFromDB<Order>(ISIN).FindAll((o)=> { return o.UserName == UserName; });
                            var sells = orders.FindAll(o => o.OrderType == ShareComponentType.sell).OrderBy(o => o.Date).ToList();
                            var buysPrior = orders.FindAll(o => o.OrderType == ShareComponentType.buy && o.Date < BookingDate).OrderBy(o => o.Date).ToList();

                            List<int> soldRemaining = new List<int>();
                            List<int> buysRemaining = new List<int>();
                            //put the amounts of sold/bought shares into a List
                            foreach (var order in sells)
                            {
                                soldRemaining.Add(order.Amount);
                            }
                            foreach (var order in buysPrior)
                            {
                                buysRemaining.Add(order.Amount);
                            }

                            //leave only the sells that are not already sold
                            for (int i = 0; i < soldRemaining.Count; i++)
                            {
                                for (int j = 0; j < buysRemaining.Count; j++)
                                {
                                    if (soldRemaining[i] <= 0)
                                    {
                                        break;
                                    }
                                    if (buysRemaining[j] <= 0)
                                    {
                                        continue;
                                    }

                                    if (soldRemaining[i] <= buysRemaining[j])
                                    {
                                        buysRemaining[j] -= soldRemaining[i];
                                        soldRemaining[i] = 0;
                                    }
                                    else
                                    {
                                        soldRemaining[i] -= buysRemaining[j];
                                        buysRemaining[j] = 0;
                                    }
                                }
                            }

                            double sum = 0.0;
                            int tempAmount = Amount;
                            // go through all sells and add up the prices
                            for (int i = 0; i < soldRemaining.Count; i++)
                            {
                                if (tempAmount <= 0) // if amount remaining for this share is 0...
                                {
                                    break; // ... we can stop the loop
                                }
                                if (soldRemaining[i] <= 0) // if the the amount for the sell is 0...
                                {
                                    continue; // ... go to the next sell
                                }

                                if (soldRemaining[i] >= tempAmount) // if the sells are more or equal the remaining amount...
                                {
                                    sum += tempAmount * sells[i].SharePrice; // ... add up the price for the remaining amount (the sharepirce of the sell)
                                    sum -= OrderExpenses * (tempAmount / Amount); // ...remove the (partial) order expenses
                                                                                  // set the remaining amounts
                                    soldRemaining[i] -= tempAmount;
                                    tempAmount = 0;
                                }
                                else // if the remaining amount is larger...
                                {
                                    sum += soldRemaining[i] * sells[i].SharePrice; // ... add up the price for the sold amount (using the sharepirce of the sell)
                                    sum -= OrderExpenses * (soldRemaining[i] / Amount); // ...remove the (partial) order expenses
                                                                                        // set the remaining amounts
                                    tempAmount -= soldRemaining[i];
                                    soldRemaining[i] = 0;
                                }
                            }

                            // app up price for the remaining (not sold) shares (using the current price)
                            sum += tempAmount * SinglePriceNow;
                            sum -= OrderExpenses * (tempAmount / Amount); // remove the (patial) order expenses

                            return sum;
                        }
                }
            }
        }

        private double _orderExpenses;
        /// <summary>
        /// The fees of the order
        /// are added once to SumBuy
        /// and once to SumNow
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
                    ComboBox cobo = null;
                    foreach (var child in sp.Children)
                    {
                        if (child.GetType() == typeof(ComboBox))
                        {
                            cobo = child as ComboBox;
                            break;
                        }
                    }

                    // create a new order
                    Order order = new Order();
                    order.Amount = Convert.ToInt32(Amount);
                    order.OrderExpenses = 10;
                    order.OrderType = ShareComponentType.buy;
                    order.SharePrice = Convert.ToDouble(SinglePriceBuy);
                    order.Date = DateTime.Today;
                    order.ISIN = (cobo.SelectedItem as ShareViewModel).ISIN;

                    // add the order to the matching share
                    DataBaseHelper.AddOrderToDB(order);
                }
            }

        }

        private bool CanAddOrder(object o)
        {
            return Amount > 0 ? true : false;
        }
        #endregion
    }
}
