using System.Collections.Generic;
using System.Windows;

namespace StockMarket
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Share> shares;
        public MainWindow()
        {
            InitializeComponent();
            shares = XmlHelper.ReadConfig();
            MainFrame.Navigate(new Pages.BlankPage());
        }

        private void B_AddShare_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.AddSharePage(ref shares));
        }

        private void B_AddOrder_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.AddOrderPage(ref shares));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            XmlHelper.SaveConfig(shares, "Config.xml");
        }

        private void B_Overview_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.ShareOverviewPage(ref shares));
        }
    }
}
