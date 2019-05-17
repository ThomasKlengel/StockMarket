using System;
using System.Collections.Generic;
using StockMarket.ViewModels;

namespace StockMarket
{
    public class Share
    {
        #region ctors
        public Share() { }

        public Share(string name, string website, string wkn, string isin)
        {
            ShareName = name;
            WebSite = website;
            WKN = wkn;
            ISIN = isin;
            Orders = new List<Order>();
            DayValues = new List<DayValue>();

        }

        public Share(string name, string website, string wkn, string isin, List<Order> orders) : this(name, website, wkn, isin)
        {
            Orders = orders;
        }

        public Share(string name, string website, string wkn, string isin, List<DayValue> values) : this(name, website, wkn, isin)
        {
            DayValues = values;
        }
        #endregion

        #region Properties

        private string _shareName;

        public string ShareName
        {
            get { return _shareName; }
            set { _shareName = value; }
        }

        private string _webSite;

        public string WebSite
        {
            get { return _webSite; }
            set { _webSite = value; }
        }

        private string _wkn;

        public string WKN
        {
            get { return _wkn; }
            set { _wkn = value; }
        }

        private string _isin;

        public string ISIN
        {
            get { return _isin; }
            set { _isin = value; }
        }

        private List<Order> _orders;

        public List<Order> Orders
        {
            get { return _orders; }
            set { _orders = value; }
        }

        private List<DayValue> _dayValues;

        public List<DayValue> DayValues
        {
            get { return _dayValues; }
            set { _dayValues = value; }
        }

        #endregion

        #region Methods
        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Share))
            {
                return this.ISIN == (obj as Share).ISIN;
            }
            return false;
        }

        public static Share CreateFromViewModel (ShareViewModel svm)
        {
            Share share = new Share(svm.ShareName,svm.WebSite,svm.WKN,svm.ISIN);
            var orders = new List<Order>();
            foreach (var order in svm.Orders)
            {
                orders.Add(new Order()                
                {
                    Date = order.Date,
                    Amount = order.Amount,
                    OrderExpenses = order.OrderExpenses,
                    OrderType = order.OrderType,
                    SharePrice = order.SharePrice
                });
            }
            share.Orders = orders;

            var dayValues = new List<DayValue>();
            foreach (var dayVal in svm.DayValues)
            {
                dayValues.Add(new DayValue()
                {
                    Date = dayVal.Date,
                    Price = dayVal.Price
                });
            }

            share.DayValues = dayValues;

            return share;

        }
        #endregion

    }

    public class Order
    {
        #region ctors
        public Order() { }
        #endregion

        #region Properties
        private double _orderPrice;

        public double SharePrice
        {
            get { return _orderPrice; }
            set { _orderPrice = value; }
        }

        private int _amount;        

        public int Amount
        {
            get { return _amount; }
            set { _amount = value; }
        }

        private DateTime _date;

        public DateTime Date
        {
            get { return _date.Date; }
            set { _date = value; }
        }

        private double _orderExpenses;

        public double OrderExpenses
        {
            get { return _orderExpenses; }
            set { _orderExpenses = value; }
        }

        private OrderType _orderType;

        public OrderType OrderType
        {
            get { return _orderType; }
            set { _orderType = value; }
        }

        #endregion
    }

    public class DayValue
    {
        #region ctors
        public DayValue() { }
        #endregion

        #region Properties
        private double _price;

        public double Price
        {
            get { return _price; }
            set { _price = value; }
        }

        private DateTime _date;

        public DateTime Date
        {
            get { return _date; }
            set { _date = value; }
        }

        #endregion

    }

    public enum OrderType
    {
        buy,
        sell
    }
}
