using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using StockMarket.ViewModels;
using StockMarket.DataModels;

namespace StockMarket.Pages
{
    /// <summary>
    /// Interaktionslogik für AddSharePage.xaml
    /// </summary>
    public partial class AddSharePage : Page
    {
        SharesDataModel _model;
        ShareViewModel _vmShare;
        public AddSharePage(ref SharesDataModel model)
        {
            InitializeComponent();
            _model = model;
            _vmShare = new ShareViewModel();

            //Datacontext for the textboxes is a share
            this.DataContext = _vmShare;
        }

        private void B_Confirm_Click(object sender, RoutedEventArgs e)
        {
            var share = new Share()
            {
                ISIN = _vmShare.ISIN,
                ShareName = _vmShare.ShareName,
                WebSite = _vmShare.WebSite,
                WKN = _vmShare.WebSite
            };
            // check if share is already in the list...
            if (!_model.Shares.Contains(share))
            {
                // ...if not, add it to the list
                _model.Shares.Add(share);
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

            //id">WKN: 623100 / ISIN: DE0006231004</span>
            //var test= "nbsp;<span class=\"instrument - id\">WKN: 623100 / ISIN: DE0006231004</span></h1><div"
            var idMatch = Regex.Match(webContent, RegexHelper.REGEX_Group_IDs);
            var wknMatch = Regex.Match(idMatch.Value, RegexHelper.REGEX_WKN);
            var isinMatch = Regex.Match(idMatch.Value, RegexHelper.REGEX_ISIN);
            string wkn = wknMatch.Value.Substring(5);
            string isin = isinMatch.Value.Substring(6);

            //< h2 class="box-headline">Aktienkurs Infineon AG in <span id = "jsCurrencySelect" > EUR </ span >
            var nameMatch = Regex.Match(webContent, RegexHelper.REGEX_Group_ShareName);
            var nameM2 = Regex.Match(nameMatch.Value, RegexHelper.REGEX_ShareName);
            var name = nameM2.Value.Substring(10).Trim().Replace(" in", "");

            _vmShare.ISIN = isin;
            _vmShare.ShareName = name;
            _vmShare.WKN = wkn;
            _vmShare.ActualPrice = RegexHelper.GetSharPrice(webContent);
            _vmShare.DayValues.Add(new DayValueViewModel() { Date = DateTime.Today, Price = _vmShare.ActualPrice });

        }
    }

}
