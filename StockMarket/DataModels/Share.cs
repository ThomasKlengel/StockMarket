using System;
using SQLite;

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
        }

        #endregion

        #region Properties

        public string ShareName { get; set; }

        public string WebSite { get; set; }

        public string WKN { get; set; }

        [PrimaryKey]
        public string ISIN { get; set; }
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
        #endregion

    }

    public class Order
    {
        #region ctors
        public Order() { }
        #endregion

        #region Properties

        [PrimaryKey]
        [AutoIncrement]
        public int DB_ID { get; set; }

        public double SharePrice { get; set; }

        public int Amount { get; set; }

        public DateTime Date { get; set; }

        public double OrderExpenses { get; set; }

        public OrderType OrderType { get; set; }

        public string ISIN { get; set; }

        #endregion
    }

    public class ShareValue
    {
        #region ctors
        public ShareValue() { }
        #endregion

        #region Properties

        [PrimaryKey]
        [AutoIncrement]
        public int DB_ID { get; set; }

        public double Price { get; set; }

        public DateTime Date { get; set; }

        public string ISIN { get; set; }
        #endregion

    }


    public enum OrderType
    {
        buy,
        sell
    }
}
