using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace StockMarket
{
    public class User
    {
        public User() { }

        public User(string userName)
        {
            UserName = userName;
        }

        [PrimaryKey]
        public string UserName { get; set; }

    }
}
