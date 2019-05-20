using System.Collections.Generic;
using System.Windows;

namespace StockMarket
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DataModels.SharesDataModel model;
        public MainWindow()
        {
            InitializeComponent();
            model = Helper.ReadFromDB();
            MainFrame.Navigate(new Pages.BlankPage());
        }

        private void B_AddShare_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.AddSharePage(ref model));
        }

        private void B_AddOrder_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.AddOrderPage(ref model));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Helper.SaveToDB(model);
        }

        private void B_Overview_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.ShareOverviewPage(ref model));

        }
    }
}
