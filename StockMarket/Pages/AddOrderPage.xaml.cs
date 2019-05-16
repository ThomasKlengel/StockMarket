using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace StockMarket.Pages
{
    /// <summary>
    /// Interaktionslogik für AddOrderPage.xaml
    /// </summary>
    public partial class AddOrderPage : Page
    {
        List<Share> shares;
        public AddOrderPage(ref List<Share> shares, ref Frame mainFrame)
        {
            InitializeComponent();
            this.shares = shares;

            foreach (var share in shares)
            {
                CoBo_AG.Items.Add(share.ShareName);
            }
        }

        private void CoBo_AG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var items = e.AddedItems;
            var item = items[0].ToString();

            var website = from share in shares
                          where share.ShareName == item
                          select share.WebSite;

            string retStr = string.Empty;
            using (WebClient client = new WebClient())
            {
                retStr = client.DownloadString(website.First());
            }

            if (retStr == string.Empty)
            {
                return;
            }

            //<tr><td class="font-bold">Kurs</td><td colspan="4">18,25 EUR<span
            var kursMatch = Regex.Match(retStr, "\\<tr\\>\\<td class=\"font-bold\"\\>Kurs\\<.*EUR.*\\<span");
            if (!kursMatch.Success)
            {
                return;
            }
            string kurs = Regex.Match(kursMatch.Value, "\\d*\\.?\\d*,\\d*").Value;

            TB_Price.Text = kurs;
        }

        private void B_AddOrder_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (TB_Amount.Text!=string.Empty)
            {
                var share = from s in shares
                            where s.ShareName == CoBo_AG.SelectedValue.ToString()
                            select s;

                Order o = new Order();
                o.Amount = Convert.ToInt32(TB_Amount.Text);
                o.OrderExpenses = 10;
                o.OrderType = OrderType.buy;
                o.SharePrice = Convert.ToDouble(TB_Price.Text);
                o.Date = DateTime.Today;
                shares.First().Orders.Add(o);
            }
        }
    }
}
