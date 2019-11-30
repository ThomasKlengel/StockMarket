using SQLite;

namespace StockMarket
{
    /// <summary>
    /// A class which represents a user.
    /// </summary>
    public class User
    {
        /// <summary>
        /// returns a default instance ("All Users").
        /// </summary>
        /// <returns></returns>
        public static User Default()
        {
            return new User("All", "Users");
        }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        public User()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        public User(string firstName, string lastName)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
        }

        #endregion

        #region Properties

        [PrimaryKey][AutoIncrement]
        public int DB_ID { get; set; }

        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        public string FirstName { get;  set; }

        /// <summary>
        /// Gets or sets the last name of the user.
        /// </summary>
        public string LastName { get;  set; }

        #endregion

        #region Methods

        /// <summary>
        /// returns the first name and the last name sepereated by a white space.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{this.FirstName} {this.LastName}";
        }

        /// <summary>
        /// compares the user by its string representation.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            // check if it is a share to compare
            if (obj.GetType() == typeof(User))
            {
                // compare the string values (first+last name)
                return this.ToString() == (obj as User).ToString();
            }

            return false;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
