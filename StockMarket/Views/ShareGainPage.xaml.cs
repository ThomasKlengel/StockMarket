using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System.Windows.Controls;
using System.Windows.Input;

namespace StockMarket.Views
{
    /// <summary>
    /// Interaktionslogik für OrdersGainPage2.xaml.
    /// </summary>
    public partial class ShareGainPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShareGainPage"/> class.
        /// </summary>
        public ShareGainPage()
        {
            this.InitializeComponent();
        }

        // TODO: try to move this to the view model
        private void CoBo_AG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // zoom to complete chart
            if (Chart.Series.Count > 2)
            {
                if (Chart.Series[Chart.Series.Count - 1] is ScatterSeries orders)
                {
                    if (orders.Values[0] is DateTimePoint point)
                    {
                        Chart.AxisX[0].MinValue = point.DateTime.Ticks;
                    }
                }

                if (Chart.Series[1] is LineSeries values)
                {
                    if (values.Values[values.Values.Count - 1] is DateTimePoint point)
                    {
                        Chart.AxisX[0].MaxValue = point.DateTime.Ticks;
                    }
                }
            }
        }

        private void Chart_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CoBo_AG_SelectionChanged(null, null);
        }
    }
}
