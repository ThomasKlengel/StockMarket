using IronOcr;
using Microsoft.Win32;
using Prism.Events;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for a single <see cref="Order"/>
    /// </summary>
    public class AddUserViewModel : ViewModelBase
    {
        #region ctor
        public AddUserViewModel()
        {
            AddOrderCommand = new RelayCommand(AddUser, CanAddUser);
            Users = new ObservableCollection<User>();
            Users.Clear();
            foreach (var user in DataBaseHelper.GetUsersFromDB())
            {
                Users.Add(user);
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
            DataBaseHelper.AddUserToDB(new User(FirstName,LastName));
            foreach (var user in DataBaseHelper.GetUsersFromDB())
            {
                if (!Users.Any((u) => { return u.Equals(user); }))
                {
                    Users.Add(user);
                }
            }
        }

        private bool CanAddUser(object o)
        {
            return !Users.Any((u) => { return u.Equals(new User(FirstName, LastName)); }); ;
        }

        #endregion
    }
}
