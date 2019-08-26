using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace StockMarket
{
    class Saver
    {
        public static void Save()
        {
            var users = new List<User>();
            var orders = new List<List<Order>>();
            var shares = new List<Share>();
            var values = new List<List<ShareValue>>();

            var UserSerializer = new XmlSerializer(typeof(List<User>));
            var ShareSerializer = new XmlSerializer(typeof(List<Share>));
            var OrderSerializer = new XmlSerializer(typeof(List<List<Order>>));
            var ValueSerializer = new XmlSerializer(typeof(List<List<ShareValue>>));
            var UserWriter = new StreamWriter("SavedUser.xml");
            var ShareWriter = new StreamWriter("SavedShare.xml");
            var OrderWriter = new StreamWriter("SavedOrder.xml");
            var ValueWriter = new StreamWriter("SavedValue.xml");

            UserSerializer.Serialize(UserWriter, DataBaseHelper.GetUsersFromDB());
            ShareSerializer.Serialize(ShareWriter, DataBaseHelper.GetSharesFromDB());
            foreach (var share in DataBaseHelper.GetSharesFromDB())
            {
                orders.Add(DataBaseHelper.GetItemsFromDB<Order>(share));
                values.Add(DataBaseHelper.GetItemsFromDB<ShareValue>(share));

            }
            OrderSerializer.Serialize(OrderWriter, orders);
            ValueSerializer.Serialize(ValueWriter, values);

            UserWriter.Close();
            ValueWriter.Close();
            OrderWriter.Close();
            ShareWriter.Close();            
        }

        public static void Load()
        {
            var OrderSerializer = new XmlSerializer(typeof(List<List<Order>>));
            var reader = new StreamReader("SavedOrder.xml");

            var deser = OrderSerializer.Deserialize(reader);
        }
    }
}
