using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using StockMarket.ViewModels;
using StockMarket.DataModels;

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
            var svm = e.AddedItems[0] as ShareViewModel;

            var retstr = string.Empty;
            using (WebClient client = new WebClient())
            {
                retstr = client.DownloadString(svm.WebSite);
            }

            var group = Regex.Match(retstr, RegexHelper.REGEX_Group_SharePrice);
            var price = Regex.Match(group.Value, RegexHelper.REGEX_SharePrice).Value.Replace(".", "");



            foreach (var order in svm.Orders)
            {
                order.ActPrice = Convert.ToDouble(price);
            }

            // set the share as DataContext for the ListView
            LV.DataContext = svm;

        }
    }
}
