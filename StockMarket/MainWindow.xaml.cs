using System.Collections.Generic;
using System.Windows;

namespace StockMarket
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new Pages.BlankPage());
        }

        private void B_AddShare_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.AddSharePage());
        }

        private void B_AddOrder_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.AddOrderPage());
        }

        private void B_Overview_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.OrdersOverviewPage());
        }
    }
}
