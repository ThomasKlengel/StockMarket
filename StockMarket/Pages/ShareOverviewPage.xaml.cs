using System;
using System.Net;
using System.Windows.Controls;
using StockMarket.ViewModels;
using StockMarket.DataModels;
using System.Windows.Threading;
using System.Linq;
using System.Collections.Generic;

namespace StockMarket.Pages
{
    /// <summary>
    /// Interaktionslogik für ShareOverviewPage.xaml
    /// </summary>
    public partial class ShareOverviewPage : Page
    {
        DispatcherTimer refrehTimer;
        SharesDataModel _model;
        public ShareOverviewPage(ref SharesDataModel model)
        {
            InitializeComponent();

            _model = model;

            // We need to populate the comboboxItems with ShareNames
            // so DataContext for Combobox is the MainViemodel which contains all Shares (and their names)
            CoBo_AG.DataContext = MainViewModel.PopulateFromModel(_model);
            CoBo_AG.SelectedIndex = 0;                   

            refrehTimer = new DispatcherTimer();
            refrehTimer.Interval = new TimeSpan(0,10,0);
            refrehTimer.Tick += RefrehTimer_Tick;
            refrehTimer.Start();           

        }

        private void RefrehTimer_Tick(object sender, EventArgs e)
        {
            //refresh the actual prices
            RefreshPrice(CoBo_AG.SelectedItem as ShareViewModel);
        }

        private void CoBo_AG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // get the selected Share
            var svm = e.AddedItems[0] as ShareViewModel;

            // refresh the actual prices
            RefreshPrice(svm);

        }

        /// <summary>
        /// refreshes the actual prices for the orders shown on the page
        /// </summary>
        /// <param name="svm">The ViewModel containing the shown orders</param>
        private void RefreshPrice(ShareViewModel svm)
        {
            // get the actual price from the website
            var webContent = string.Empty;
            using (WebClient client = new WebClient())
            {
                webContent = client.DownloadString(svm.WebSite);
            }

            double price = RegexHelper.GetSharPrice(webContent);

            // set the price for each order, since they are of the same ISIN
            foreach (var order in svm.Orders)
            {
                order.ActPrice = Convert.ToDouble(price);
            }

            // set the share as DataContext for the ListView
            LV.DataContext = svm;


            // create a new viewmodel for the overview, containing the new order data
            OrderOverviewViewModel oovm = new OrderOverviewViewModel();
            oovm.ActPrice = price;

            var orders = from order in svm.Orders
                         select order;

            oovm.Orders = orders.ToList<OrderViewModel>();

            // set the viewmodel as data context of the grid
            OverViewGrid.DataContext = oovm;
        }

        private void Page_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            refrehTimer.Stop();
        }
    }
}
