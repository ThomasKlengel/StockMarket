using System.Collections.Generic;

namespace StockMarket
{
    /// <summary>
    /// A datamodel for <see cref="Share"/>s containing lists for 
    /// <see cref="Share"/>s, <see cref="Order"/>s and <see cref="ShareValue"/>s
    /// Only used for saving the database to an xml file
    /// </summary>
    public class SharesDataModel
    {
        /// <summary>
        /// A list of <see cref="Share"/>s within a database
        /// </summary>
        public List<Share> Shares { get; set; }

        /// <summary>
        /// A list of <see cref="Order"/>s within a database
        /// </summary>
        public List<Order> Orders { get; set; }

        /// <summary>
        /// A list of <see cref="ShareValue"/>s within a database
        /// </summary>
        public List<ShareValue> ShareValues { get; set; }
    }
}
