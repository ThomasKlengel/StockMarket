using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using StockMarket.ViewModels;
using StockMarket.DataModels;

namespace StockMarket.Pages
{
    /// <summary>
    /// Interaktionslogik für AddOrderPage.xaml
    /// </summary>
    public partial class AddOrderPage : Page
    {
        SharesDataModel _model;
        OrderViewModel _vmOrder;
        public AddOrderPage(ref SharesDataModel model)
        {
            InitializeComponent();
            _model = model;
            _vmOrder = new OrderViewModel();

            // Datacontext for textboxes is an Order
            this.DataContext = _vmOrder;

            // We need to populate the comboboxItems with ShareNames
            // so DataContext for Combobox is the MainViemodel which contains all Shares (and their names)
            CoBo_AG.DataContext = MainViewModel.PopulateFromModel(model);

        }

        private void CoBo_AG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // get the website of the selected Share
            var website = from share in _model.Shares
                          where share.ShareName == (e.AddedItems[0] as ShareViewModel).ShareName 
                          select share.WebSite;

            string webContent = string.Empty;

            // try to get the website content
            try
            {
                using (WebClient client = new WebClient())
                {
                    webContent = client.DownloadString(website.First());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            if (webContent == string.Empty)
            {
                return;
            }

            _vmOrder.SharePrice = RegexHelper.GetSharPrice(webContent);
        }

        private void B_AddOrder_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_vmOrder.Amount>0)
            {
                // create a new order
                Order o = new Order();
                o.Amount = Convert.ToInt32(_vmOrder.Amount);
                o.OrderExpenses = 10;
                o.OrderType = OrderType.buy;
                o.SharePrice = Convert.ToDouble(_vmOrder.SharePrice);
                o.Date = DateTime.Today;
                o.ISIN = (CoBo_AG.SelectedItem as ShareViewModel).ISIN;

                // add the order to the matched share
                _model.Orders.Add(o);
            }
        }
    }
}
