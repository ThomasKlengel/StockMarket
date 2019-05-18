using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using StockMarket.ViewModels;

namespace StockMarket.Pages
{
    /// <summary>
    /// Interaktionslogik für AddSharePage.xaml
    /// </summary>
    public partial class AddSharePage : Page
    {
        List<Share> _shares;
        ShareViewModel _vmShare;
        public AddSharePage(ref List<Share> shares)
        {
            InitializeComponent();
            _shares = shares;
            _vmShare = new ShareViewModel();

            //Datacontext for the textboxes is a share
            this.DataContext = _vmShare;
        }

        private void B_Confirm_Click(object sender, RoutedEventArgs e)
        {
            var share = Share.CreateFromViewModel(_vmShare);
            // check if share is already in the list...
            if (!_shares.Contains(share))
            {                 
                // ...if not, add it to the list
                _shares.Add(share);
            }
            else
            {
                MessageBox.Show($"You already have a share with an ISIN matching ISIN={share.ISIN}");
            }
        }

        private void B_AutoFill_Click(object sender, RoutedEventArgs e)
        {
            if (_vmShare.WebSite==String.Empty)
            {
                return;
            }

            string webContent = string.Empty;

            // try to get the website content
            try
            {
                using (WebClient client = new WebClient())
                {
                    // https://www.finanzen.net/aktien...
                    webContent = client.DownloadString(_vmShare.WebSite);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            if (webContent == string.Empty)
            {
                return;
            }

            // get the section containing the share price
            //<tr><td class="font-bold">Kurs</td><td colspan="4">18,25 EUR<span
            var priceMatch = Regex.Match(webContent, Helpers.REGEX_Group_SharePrice);
            if (!priceMatch.Success)
            {
                return;
            }
            //
            string sharePrice = Regex.Match(priceMatch.Value, Helpers.REGEX_SharePrice).Value.Replace(".","");

            //id">WKN: 623100 / ISIN: DE0006231004</span>
            //var test= "nbsp;<span class=\"instrument - id\">WKN: 623100 / ISIN: DE0006231004</span></h1><div"
            var idMatch = Regex.Match(webContent, Helpers.REGEX_Group_IDs);
            var wknMatch = Regex.Match(idMatch.Value, Helpers.REGEX_WKN);
            var isinMatch = Regex.Match(idMatch.Value, Helpers.REGEX_ISIN);
            string wkn = wknMatch.Value.Substring(5);
            string isin = isinMatch.Value.Substring(6);

            //< h2 class="box-headline">Aktienkurs Infineon AG in <span id = "jsCurrencySelect" > EUR </ span >
            var nameMatch = Regex.Match(webContent, Helpers.REGEX_Group_ShareName);
            var nameM2 = Regex.Match(nameMatch.Value, Helpers.REGEX_ShareName);
            var name = nameM2.Value.Substring(10).Trim().Replace(" in", "");

            _vmShare.ISIN = isin;
            _vmShare.ShareName = name;
            _vmShare.WKN = wkn;
            _vmShare.ActualPrice = Convert.ToDouble(sharePrice);
            _vmShare.DayValues.Add(new DayValueViewModel() { Date = DateTime.Today, Price = _vmShare.ActualPrice });

        }
    }

}
