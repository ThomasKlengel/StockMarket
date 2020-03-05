using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for a single <see cref="Order"/>.
    /// </summary>
    public class OrderViewModel : ShareComponentViewModel
    {
        #region ctor

        /// <summary>
        /// Creates a ViewModel for an order with an AddOrderCommand.
        /// </summary>
        public OrderViewModel()
            : base()
        {
            this.AddOrderCommand = new RelayCommand(this.AddOrder, this.CanAddOrder);
        }

        /// <summary>
        /// Creates a ViewModel for an order from an order.
        /// </summary>
        /// <param name="order">The <see cref="Order"/> to create a ViewModel for.</param>
        public OrderViewModel(Order order)
            : base()
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

        /// <inheritdoc/>
        public override double AmountSold
        {
            get
            {
                switch (this.ComponentType)
                {
                    case ShareComponentType.Buy:
                        {
                            // get buys and sells
                            var orders = DataBaseHelper.GetItemsFromDB<Order>(this.ISIN);
                            var sells = orders.FindAll(o => o.OrderType == ShareComponentType.Sell).OrderBy(o => o.Date).ToList();
                            var buysPrior = orders.FindAll(o => o.OrderType == ShareComponentType.Buy && o.Date < this.BookingDate).OrderBy(o => o.Date).ToList();

                            // sum up the amount of sold shares
                            double numSells = 0;
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

                            return this.Amount < numSells ? this.Amount : numSells;
                        }

                    default: return this.Amount;
                }
            }
        }

        /// <summary>
        /// Gets the summed up price of the shares at the day of purchase
        /// (getter only).
        /// </summary>
        public override double SumBuy
        {
            get
            {
                switch (this.ComponentType)
                {
                    case ShareComponentType.Sell:
                        {
                            var orders = DataBaseHelper.GetItemsFromDB<Order>(this.ISIN);

                            // get buys and sells
                            var sellsPrior = orders.FindAll(o => o.OrderType == ShareComponentType.Sell && o.Date < this.BookingDate).OrderBy(o => o.Date).ToList();
                            var buys = orders.FindAll(o => o.OrderType == ShareComponentType.Buy).OrderBy(o => o.Date).ToList();

                            List<double> soldRemaining = new List<double>();
                            List<double> buysRemaining = new List<double>();
                            // put the amounts of sold/bought shares into a List
                            foreach (var order in sellsPrior)
                            {
                                soldRemaining.Add(order.Amount);
                            }

                            foreach (var order in buys)
                            {
                                buysRemaining.Add(order.Amount);
                            }

                            // leave only the sells that are not already sold
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
                            double tempAmount = this.Amount;
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
                            sum += tempAmount * this.SinglePriceNow;
                            sum += this.OrderExpenses * (tempAmount / this.Amount); // remove the (patial) order expenses

                            return sum;
                        }

                    default: // buys, for safety as default
                        {
                            var orders = DataBaseHelper.GetItemsFromDB<Order>(this.ISIN);
                            double sum = 0;
                            // if it is an ETF...
                            if (DataBaseHelper.GetItemsFromDB<Share>(this.ISIN).First().ShareType == ShareType.ETF)
                            {   // ... go through each order for the share
                                foreach (var order in orders)
                                {   // if it is reinvesting
                                    if (order.ReInvesting)
                                    {   // accumulate since orderdate
                                        var monthSinceBuy = DateTime.Today.Date.Month - orders.First().Date.Month;
                                        sum += orders.First().Amount * orders.First().SharePrice * monthSinceBuy;
                                    }
                                    else
                                    {   // sum is the same as for regular share
                                        sum += (order.SharePrice * order.Amount) + this.OrderExpenses;
                                    }
                                }
                            }
                            else
                            {
                                sum = (this.SinglePriceBuy * this.Amount) + this.OrderExpenses;
                            }

                            return sum;
                        }
                }
            }
        }

        //TODO: recalculate the current value
        /// <summary>
        /// Gets the current summed up price of the shares
        /// (getter only).
        /// </summary>
        public override double SumNow
        {
            get
            {
                switch (this.ComponentType)
                {
                    case ShareComponentType.Sell: return (this.SinglePriceBuy * this.AmountSold) + this.OrderExpenses;
                    default: // buys, default for safety
                        {
                            // get buys and sells
                            var orders = DataBaseHelper.GetItemsFromDB<Order>(this.ISIN).FindAll((o) => { return o.UserName == this.UserName; });
                            var sells = orders.FindAll(o => o.OrderType == ShareComponentType.Sell).OrderBy(o => o.Date).ToList();
                            var buysPrior = orders.FindAll(o => o.OrderType == ShareComponentType.Buy && o.Date < this.BookingDate).OrderBy(o => o.Date).ToList();

                            List<double> soldRemaining = new List<double>();
                            List<double> buysRemaining = new List<double>();
                            // put the amounts of sold/bought shares into a List
                            foreach (var order in sells)
                            {
                                soldRemaining.Add(order.Amount);
                            }

                            foreach (var order in buysPrior)
                            {
                                buysRemaining.Add(order.Amount);
                            }

                            // leave only the sells that are not already sold
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
                            double tempAmount = this.Amount;
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
                                    sum -= this.OrderExpenses * (tempAmount / this.Amount); // ...remove the (partial) order expenses
                                                                                  // set the remaining amounts
                                    soldRemaining[i] -= tempAmount;
                                    tempAmount = 0;
                                }
                                else // if the remaining amount is larger...
                                {
                                    sum += soldRemaining[i] * sells[i].SharePrice; // ... add up the price for the sold amount (using the sharepirce of the sell)
                                    sum -= this.OrderExpenses * (soldRemaining[i] / this.Amount); // ...remove the (partial) order expenses
                                                                                        // set the remaining amounts
                                    tempAmount -= soldRemaining[i];
                                    soldRemaining[i] = 0;
                                }
                            }

                            // app up price for the remaining (not sold) shares (using the current price)
                            sum += tempAmount * this.SinglePriceNow;
                            sum -= this.OrderExpenses * (tempAmount / this.Amount); // remove the (patial) order expenses

                            return sum;
                        }
                }
            }
        }

        private double _orderExpenses;

        /// <summary>
        /// Gets or sets the fees of the order
        /// are added once to SumBuy
        /// and once to SumNow.
        /// </summary>
        public double OrderExpenses
        {
            get
            {
                return this._orderExpenses;
            }

            set
            {
                if (this._orderExpenses != value)
                {
                    this._orderExpenses = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.OrderExpenses)));
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
                    order.Amount = Convert.ToInt32(this.Amount);
                    order.OrderExpenses = 10;
                    order.OrderType = ShareComponentType.Buy;
                    order.SharePrice = Convert.ToDouble(this.SinglePriceBuy);
                    order.Date = DateTime.Today;
                    order.ISIN = (cobo.SelectedItem as AddShareViewModel).ISIN;

                    // add the order to the matching share
                    DataBaseHelper.AddOrderToDB(order);
                }
            }
        }

        private bool CanAddOrder(object o)
        {
            return this.Amount > 0 ? true : false;
        }
        #endregion
    }
}
