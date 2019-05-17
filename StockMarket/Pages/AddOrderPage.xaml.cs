using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using StockMarket.ViewModels;

namespace StockMarket.Pages
{
    /// <summary>
    /// Interaktionslogik für AddOrderPage.xaml
    /// </summary>
    public partial class AddOrderPage : Page
    {
        List<Share> shares;
        OrderViewModel _vmOrder;
        public AddOrderPage(ref List<Share> shares)
        {
            InitializeComponent();
            this.shares = shares;
            _vmOrder = new OrderViewModel();

            // Datacontext for textboxes is an Order
            this.DataContext = _vmOrder;

            // We need to populate the comboboxItems with ShareNames
            // so DataContext for Combobox is the MainViemodel which contains all Shares (and their names)
            CoBo_AG.DataContext = MainViewModel.PopulateFromShares(shares);

        }

        private void CoBo_AG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // get the website of the selected Share
            var website = from share in shares
                          where share.ShareName == (e.AddedItems[0] as ShareViewModel).ShareName 
                          select share.WebSite;

            string retStr = string.Empty;

            // try to get the website content
            try
            {
                using (WebClient client = new WebClient())
                {
                    retStr = client.DownloadString(website.First());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            if (retStr == string.Empty)
            {
                return;
            }


            // get the section of the website which contains the SharePrice
            //<tr><td class="font-bold">Kurs</td><td colspan="4">18,25 EUR<span
            var priceMatch = Regex.Match(retStr, Helpers.REGEX_Group_SharePrice);
            if (!priceMatch.Success)
            {
                return;
            }
            // get the SharePrice in the desired format
            string sharePrice = Regex.Match(priceMatch.Value, Helpers.REGEX_SharePrice).Value.Replace(".", "").Replace(",", ".");

            _vmOrder.SharePrice = Convert.ToDouble(sharePrice);
        }

        private void B_AddOrder_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_vmOrder.Amount>0)
            {
                // get the share which matches the selected item of the combobox
                var share = from s in shares
                            where s.ISIN == (CoBo_AG.SelectedValue as ShareViewModel).ISIN
                            select s;

                // create a new order
                Order o = new Order();
                o.Amount = Convert.ToInt32(_vmOrder.Amount);
                o.OrderExpenses = 10;
                o.OrderType = OrderType.buy;
                o.SharePrice = Convert.ToDouble(_vmOrder.SharePrice);
                o.Date = DateTime.Today;

                // add the order to the matched share
                share.First().Orders.Add(o);
            }
        }
    }
}
