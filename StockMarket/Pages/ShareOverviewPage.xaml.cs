using System;
using System.Net;
using System.Windows.Controls;
using StockMarket.ViewModels;
using StockMarket.DataModels;
using System.Linq;

namespace StockMarket.Pages
{
    /// <summary>
    /// Interaktionslogik für ShareOverviewPage.xaml
    /// </summary>
    public partial class ShareOverviewPage : Page
    {
        SharesDataModel _model;
        public ShareOverviewPage(ref SharesDataModel model)
        {
            InitializeComponent();
            _model = model;

            // We need to populate the comboboxItems with ShareNames
            // so DataContext for Combobox is the MainViemodel which contains all Shares (and their names)
            CoBo_AG.DataContext = MainViewModel.PopulateFromModel(_model);
            CoBo_AG.SelectedIndex = 0;

        }

        private void CoBo_AG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // get the selected Share
            var svm = e.AddedItems[0] as ShareViewModel;

            var webContent = string.Empty;
            using (WebClient client = new WebClient())
            {
                webContent = client.DownloadString(svm.WebSite);
            }

            double price = RegexHelper.GetSharPrice(webContent);

            foreach (var order in svm.Orders)
            {
                order.ActPrice = Convert.ToDouble(price);
            }

            // set the share as DataContext for the ListView
            LV.DataContext = svm;

            OrderOverviewViewModel oovm = new OrderOverviewViewModel();
            oovm.ActPrice = price;

            var orders = from order in svm.Orders
                          select order;

            oovm.Orders = orders.ToList<OrderViewModel>();

            Overview_Grid.DataContext = oovm;

        }
    }
}
