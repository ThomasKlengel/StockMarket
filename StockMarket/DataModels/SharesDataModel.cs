using System.Collections.Generic;

namespace StockMarket.DataModels
{
    public class SharesDataModel
    {
        public List<Share> Shares { get; set; }
        public List<Order> Orders { get; set; }
        public List<ShareValue> ShareValues { get; set; }
    }
}
