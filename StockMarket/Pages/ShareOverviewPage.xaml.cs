using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using StockMarket.ViewModels;

namespace StockMarket.Pages
{
    /// <summary>
    /// Interaktionslogik für ShareOverviewPage.xaml
    /// </summary>
    public partial class ShareOverviewPage : Page
    {
        List<Share> shares;
        MainViewModel _vm;
        public ShareOverviewPage(ref List<Share> shares)
        {
            InitializeComponent();
            this.shares = shares;
            _vm = new MainViewModel();

            foreach (var share in shares)
            {
                CoBo_AG.Items.Add(share.ShareName);
            }
        }

        private void CoBo_AG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
