using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using StockMarket.ViewModels;

namespace StockMarket
{
    public class User
    {
        public static readonly User Default = new User("All", "Users"); 

        public User() { FirstName = Default.FirstName; LastName = Default.LastName; }

        public User(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        [PrimaryKey][AutoIncrement]
        public int DB_ID { get; set; }

        public string FirstName { get;  set; }
        public string LastName { get;  set; }

        public override string ToString()
        {
            return $"{FirstName} {LastName}";
        }

        public override bool Equals(object obj)
        {
            // check if it is a share to compare
            if (obj.GetType() == typeof(User))
            {
                // compare the ISIN
                return this.ToString() == (obj as User).ToString();
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
