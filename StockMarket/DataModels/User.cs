using SQLite;

namespace StockMarket
{
    /// <summary>
    /// A class which represents a user
    /// </summary>
    public class User
    {
        /// <summary>
        /// returns a default instance ("All Users")
        /// </summary>
        /// <returns></returns>
        public static User Default() { return new User("All", "Users"); }

        public User() { }

        public User(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        [PrimaryKey][AutoIncrement]
        public int DB_ID { get; set; }

        /// <summary>
        /// The first name of the user
        /// </summary>
        public string FirstName { get;  set; }
        /// <summary>
        /// The last name of the user
        /// </summary>
        public string LastName { get;  set; }

        /// <summary>
        /// returns the first name and the last name sepereated by a white space
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{FirstName} {LastName}";
        }

        /// <summary>
        /// compares the user by its string representation
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
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
