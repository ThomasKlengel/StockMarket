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
    }
}
