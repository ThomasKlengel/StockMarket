using SQLite;
using System;

namespace StockMarket
{
    public class NewShare
    {
        #region ctors
        public NewShare() { }

        public NewShare(string name, string website, string wkn, string isin)
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

    public class NewOrder
    {
        #region ctors
        public NewOrder() { }
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

    public class NewDayValue
    {
        #region ctors
        public NewDayValue() { }
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
}
