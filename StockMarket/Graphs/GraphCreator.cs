using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveCharts;
using LiveCharts.Wpf;
using StockMarket.ViewModels;

namespace StockMarket.Graphs
{
    class GraphCreator
    {
        public static void Create(Share share)
        {
            // get the relevant values of the share
            var shareValues = DataBaseHelper.GetItemsFromDB<ShareValue>(share).OrderBy((s) => s.Date);
            var orders = DataBaseHelper.GetItemsFromDB<Order>(share).OrderBy((s) => s.Date);
            LiveCharts.SeriesCollection Graphs = new SeriesCollection();
                                    
            // create the series needed for a single Share
            var OrderSeries = new LineSeries() { Title = "Orders" };
            var AbsoluteSeries = new LineSeries() { Title = "Absolute Value" };
            var GrowthSeries = new LineSeries() { Title = "Growth" };

            // fill the data of the OrderSeries
            var values = new ChartValues<LiveCharts.Defaults.DateTimePoint>();
            foreach (var order in orders)
            {
                double value = order.Amount * order.SharePrice;
                OrderSeries.Values.Add(new LiveCharts.Defaults.DateTimePoint(order.Date, value));
            }
            Graphs.Add(OrderSeries);

            // fill the data of the AbsoluteSeries
            //TODO: get the value of the orders at the date of the shareValue
            values = new ChartValues<LiveCharts.Defaults.DateTimePoint>();
            foreach (var shareValue in shareValues)
            {
                GraphShareModel model = new GraphShareModel(share, shareValue.Price, shareValue.Date);                
                AbsoluteSeries.Values.Add(new LiveCharts.Defaults.DateTimePoint(shareValue.Date, model.SumNow));
            }
            Graphs.Add(AbsoluteSeries);

        }
    }
}
