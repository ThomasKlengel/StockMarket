using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for all <see cref="Order"/>s of a <see cref="Share"/>
    /// </summary>
    class SingleShareGainPageViewModel : ShareComponentViewModel
    {

        //TODO: rename ViewModel, since its essentially the VieModel for a "ShareGainViewModel"
        //create an additional one for the page it is displayed on, inheriting from this one


        #region Constructors
        public SingleShareGainPageViewModel():base()
        {
            Shares = new ObservableCollection<Share>();
        }

        public override void UserChanged()
        {
            // gets all orders for the current user
            var ordersByUser = DataBaseHelper.GetAllItemsFromDB<Order>().Where(o=>(SelectByUser(o)));
            // add any isin of these orders (HashSet only allows uniques -> duplicates are not added)
            var unique = new HashSet<string>();
            foreach (var order in ordersByUser)
            {
                unique.Add(order.ISIN);
            }
            // clear the colections
            Shares.Clear();
            var unsortedShares = new ObservableCollection<Share>();
            // get all shares for the user
            foreach (var isin in unique)
            {
                unsortedShares.Add(DataBaseHelper.GetItemsFromDB<Share>(isin).First());
            }
            // sort the shares by name
            foreach (var share in unsortedShares.OrderBy((s) => { return s.ShareName; }))
            {
                Shares.Add(share);
            }

            if (Shares.Count > 0)
            {
                // set the selected share for initially creating the view model
                SelectedShare = Shares.First();
            }

        }

        #endregion

        #region Properties           

        /// <summary>
        /// All orders of the selected share
        /// </summary>
        public ShareViewModel DisplayedShare { get; set; }

        /// <summary>
        /// The <see cref="Share"/>s that are currently managed in the database
        /// </summary>
        public ObservableCollection<Share> Shares { get; private set; }

        private Share _selectedShare;
        /// <summary>
        /// The <see cref="Share"/> that is currently selected
        /// </summary>
        public Share SelectedShare
        {
            get { return _selectedShare; }
            set
            {
                if (_selectedShare != value)
                {
                    _selectedShare = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedShare)));

                    if (SelectedShare != null)
                    {
                        DisplayedShare = new ShareViewModel(SelectedShare);
                        // notify Share of CurrentUser so values are refreshed
                        ApplicationService.Instance.EventAggregator.GetEvent<UserChangedEvent>().Publish(CurrentUser);
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(DisplayedShare)));
                    }
                }
            }
        }

        public override double SumNow { get { return 0.0; } }

        public override double SumBuy { get { return 0.0; } }

        public override int AmountSold { get { return 0; } }

        #endregion


        #region Commands

        //private void SortOrders(object o)
        //{
        //    if (ShareComponents.Count > 1)
        //    {
        //        // check if clicked item is a column header
        //        if (o.GetType() == typeof(GridViewColumnHeader))
        //        {
        //            var header = o as GridViewColumnHeader;

        //            var headerClicked = "";
        //            // if the binding is a binding...
        //            if (header.Column.DisplayMemberBinding.GetType() == typeof(Binding))
        //            { //... set the header to the bound path
        //                var content = header.Column.DisplayMemberBinding as Binding;
        //                headerClicked = content.Path.Path;
        //                if (headerClicked.Contains("Date"))
        //                {
        //                    headerClicked = "Date";
        //                }
        //            }
        //            else
        //            { //... otherwise it's amount (which is a multibinding)
        //                headerClicked = "Amount";
        //            }

        //            //get the sort Direction
        //            if (lastSortedBy == headerClicked)
        //            {
        //                lastSortAscending = !lastSortAscending;
        //            }
        //            else
        //            {
        //                lastSortAscending = false;
        //            }

        //            //sort the orders
        //            ShareComponents = SortCollection(ShareComponents, headerClicked, lastSortAscending);

        //            // set the last sorted by for next sort
        //            lastSortedBy = headerClicked;

        //        }
        //    }
        //}

        #endregion

    }
}
