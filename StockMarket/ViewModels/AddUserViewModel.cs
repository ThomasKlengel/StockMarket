using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using IronOcr;
using Microsoft.Win32;
using Prism.Events;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for a single <see cref="Order"/>.
    /// </summary>
    public class AddUserViewModel : ViewModelBase
    {
        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AddUserViewModel"/> class.
        /// </summary>
        public AddUserViewModel()
        {
            this.AddOrderCommand = new RelayCommand(this.AddUser, this.CanAddUser);
            this.Users = new ObservableCollection<User>();
            this.Users.Clear();
            foreach (var user in DataBaseHelper.GetUsersFromDB())
            {
                this.Users.Add(user);
            }
        }

        #endregion

        #region Properties

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public ObservableCollection<User> Users { get; set; }

        #endregion

        #region Commands
        public RelayCommand AddOrderCommand { get; private set; }

        private void AddUser(object o)
        {
            // add the order to the matching share
            DataBaseHelper.AddUserToDB(new User(this.FirstName,this.LastName));
            foreach (var user in DataBaseHelper.GetUsersFromDB())
            {
                if (!this.Users.Any((u) => { return u.Equals(user); }))
                {
                    this.Users.Add(user);
                }
            }
        }

        private bool CanAddUser(object o)
        {
            return !this.Users.Any((u) => { return u.Equals(new User(this.FirstName, this.LastName)); });
        }

        #endregion
    }
}
