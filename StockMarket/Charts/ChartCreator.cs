using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using StockMarket.ViewModels;

namespace StockMarket.Charts
{
    class ChartCreator
    {
        public static ChartCollection CreateCharts(Share share)
        {
            ChartCollection returnCharts = new ChartCollection();
            if (share != null)
            {
                // get the relevant values of the share
                var shareValues = DataBaseHelper.GetItemsFromDB<ShareValue>(share).OrderBy((s) => s.Date);
                var orders = DataBaseHelper.GetItemsFromDB<Order>(share).OrderBy((s) => s.Date);

                // fill the data of the OrderSeries
                foreach (var order in orders)
                {
                    double value = order.Amount * order.SharePrice;
                    returnCharts.OrderSeries.Values.Add(new DateTimePoint(order.Date, value));
                }

                // fill the data of the AbsoluteSeries and Growth Series
                //TODO: get the value of the orders at the date of the shareValue
                foreach (var shareValue in shareValues)
                {
                    GraphShareModel model = new GraphShareModel(share, shareValue.Price, shareValue.Date);
                    returnCharts.AbsoluteSeries.Values.Add(new DateTimePoint(shareValue.Date, model.SumNow));
                    returnCharts.GrowthSeries.Values.Add(new DateTimePoint(shareValue.Date, shareValue.Price));
                }

                var a = returnCharts.AbsoluteSeries.Values[0];
                var b = returnCharts.GrowthSeries.Values[0];
                var c = returnCharts.OrderSeries.Values[0];
            }
            return returnCharts;
        }             
    }

    public class ChartCollection
    {
        public ChartCollection()
        {
            OrderSeries = new ScatterSeries() { Title = "Orders", Values = new ChartValues<DateTimePoint>() };
            AbsoluteSeries = new LineSeries() { Title = "Absolute Value", Values = new ChartValues<DateTimePoint>() };
            GrowthSeries = new LineSeries() { Title = "Growth", Values = new ChartValues<DateTimePoint>() };
        }

        public ScatterSeries OrderSeries { get; set; }
        public LineSeries AbsoluteSeries { get; set; }
        public LineSeries GrowthSeries { get; set; }
    }
}
