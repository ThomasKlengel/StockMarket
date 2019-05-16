using System;
using System.Collections.Generic;

namespace StockMarket
{
    public class Share
    {

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


        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Share))
            {
                return this.ISIN == (obj as Share).ISIN;
            }
            return false;
        }

    }

    public class Order
    {

        public Order() { }
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


    }

    public class DayValue
    {
        public DayValue() { }

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

    }


    public enum OrderType
    {
        buy,
        sell
    }
}
