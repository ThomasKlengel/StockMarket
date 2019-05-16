using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StockMarket.Pages
{
    /// <summary>
    /// Interaktionslogik für AddSharePage.xaml
    /// </summary>
    public partial class AddSharePage : Page
    {
        List<Share> shares;
        Frame mainFrame;
        public AddSharePage(ref List<Share> shares, ref Frame frame)
        {
            InitializeComponent();
            this.shares = shares;
            mainFrame = frame;
        }

        private void B_Confirm_Click(object sender, RoutedEventArgs e)
        {

            var share = new Share(TB_Name.Text, TB_WebSite.Text, TB_WKN.Text, TB_ISIN.Text);
            if (!shares.Contains(share))
            {
                share.DayValues.Add(new DayValue() { Date = DateTime.Today, Price = Convert.ToDouble(TB_Price.Text) });
                shares.Add(share);
            }
            mainFrame.Navigate(new Pages.BlankPage());
        }

        private void B_AutoFill_Click(object sender, RoutedEventArgs e)
        {
            if (TB_WebSite.Text==String.Empty)
            {
                return;
            }

            string retstr = string.Empty;

            //try
            //{
                using (WebClient client = new WebClient())
                {
                    // https://www.finanzen.net/aktien...
                    retstr = client.DownloadString(TB_WebSite.Text);
                }
            //}
            //catch () { }
            
            if (retstr == string.Empty)
            {
                return;
            }

            //<tr><td class="font-bold">Kurs</td><td colspan="4">18,25 EUR<span
            var kursMatch = Regex.Match(retstr, "\\<tr\\>\\<td class=\"font-bold\"\\>Kurs\\<.*EUR.*\\<span");
            if (!kursMatch.Success)
            {
                return;
            }
            string kurs = Regex.Match(kursMatch.Value, "\\d*\\.?\\d*,\\d*").Value;

            //id">WKN: 623100 / ISIN: DE0006231004</span>
            //var test= "nbsp;<span class=\"instrument - id\">WKN: 623100 / ISIN: DE0006231004</span></h1><div"
            var idMatch = Regex.Match(retstr, "instrument-id\"\\>.{40}");
            var wknMatch = Regex.Match(idMatch.Value, "WKN: \\d{6}");
            var isinMatch = Regex.Match(idMatch.Value, "ISIN: \\S{2}\\d{10}");
            string wkn = wknMatch.Value.Substring(5);
            string isin = isinMatch.Value.Substring(6);

            //< h2 class="box-headline">Aktienkurs Infineon AG in <span id = "jsCurrencySelect" > EUR </ span >
            var nameMatch = Regex.Match(retstr, "box-headline\"\\>Aktienkurs.{50}");
            var nameM2 = Regex.Match(nameMatch.Value, "Aktienkurs .* in");
            var name = nameM2.Value.Substring(10).Trim().Replace(" in", "");

            TB_ISIN.Text = isin;
            TB_Name.Text = name;
            TB_WKN.Text = wkn;
            TB_Price.Text = kurs;

        }
    }

}
