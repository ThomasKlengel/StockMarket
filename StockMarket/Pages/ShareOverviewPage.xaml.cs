﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using StockMarket.ViewModels;

namespace StockMarket.Pages
{
    /// <summary>
    /// Interaktionslogik für ShareOverviewPage.xaml
    /// </summary>
    public partial class ShareOverviewPage : Page
    {
        List<Share> shares;
        public ShareOverviewPage(ref List<Share> shares)
        {
            InitializeComponent();
            this.shares = shares;

            // We need to populate the comboboxItems with ShareNames
            // so DataContext for Combobox is the MainViemodel which contains all Shares (and their names)
            CoBo_AG.DataContext = MainViewModel.PopulateFromShares(shares);
            CoBo_AG.SelectedIndex = 0;

        }

        private void CoBo_AG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //select get share that matches the ComboBox
            var share = from s in shares
                        where s.ISIN == (e.AddedItems[0] as ShareViewModel).ISIN
                        select s;

            var svm = ShareViewModel.CreateFromShare(share.First());

            var retstr = string.Empty;
            using (WebClient client = new WebClient())
            {
                retstr = client.DownloadString(svm.WebSite);
            }

            var group = Regex.Match(retstr, Helpers.REGEX_Group_SharePrice);
            var price = Regex.Match(group.Value, Helpers.REGEX_SharePrice).Value.Replace(".", "");



            foreach (var order in svm.Orders)
            {
                order.ActPrice = Convert.ToDouble(price);
            }

            // set the share as DataContext for the ListView
            LV.DataContext = svm;

        }
    }
}
