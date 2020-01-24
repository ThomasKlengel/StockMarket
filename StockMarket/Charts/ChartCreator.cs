using System;
using System.Linq;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;

namespace StockMarket.Charts
{
    class ChartCreator
    {

        /// <summary>
        /// Creates the default charts (Order, Dividend, AbsoluteValue, PercentageGrowth) for the given <see cref="Share"/>
        /// </summary>
        /// <param name="share">The share to get the charts for</param>
        /// <returns></returns>
        public static ChartCollection CreateCharts(Share share)
        {
            ChartCollection returnCharts = new ChartCollection();
            if (share != null)
            {
                // get the relevant values of the share                
                var orders = DataBaseHelper.GetItemsFromDB<Order>(share).OrderBy((s) => s.Date);
                var shareValues = DataBaseHelper.GetItemsFromDB<ShareValue>(share).Where((s)=> s.Date>= orders.FirstOrDefault().Date.Date).OrderBy((s) => s.Date);
                var dividends = DataBaseHelper.GetItemsFromDB<Dividend>(share).OrderBy((s) => s.DayOfPayment);

                // fill the data of the OrderSeries
                foreach (var order in orders)
                {
                    double value = order.Amount * order.SharePrice;
                    returnCharts.OrderSeries.Values.Add(new DateTimePoint(order.Date.Date, value));
                }

                // fill the data of the DividendSeries
                foreach (var div in dividends)
                {
                    returnCharts.DividendSeries.Values.Add(new DateTimePoint(div.DayOfPayment.Date, div.Value));
                }

                // fill the data of the AbsoluteSeries and Growth Series
                //TODO: get the value of the orders at the date of the shareValue
                var first = orders.FirstOrDefault();
                foreach (var shareValue in shareValues)
                {
                    // create a model which automatically calculates the correct value
                    GraphShareModel model = new GraphShareModel(share, shareValue.Price, shareValue.Date);
                    // add the dividends to the share value
                    var completeValue = model.SumNow;
                    foreach (var dividend in dividends.Where(s=>s.DayOfPayment<=shareValue.Date))
                    {
                        completeValue += dividend.Value;
                    }
                    returnCharts.AbsoluteSeries.Values.Add(new DateTimePoint(shareValue.Date.Date, model.SumNow));
                    // calculate the percentagewise growth
                    double percentage = Math.Round(((shareValue.Price - first.SharePrice) / first.SharePrice * 100), 3);
                    returnCharts.GrowthSeries.Values.Add(new DateTimePoint(shareValue.Date.Date, percentage));
                }

            }
            return returnCharts;
        }             
    }

    /// <summary>
    /// A class containing the basic charts for a single share
    /// </summary>
    public class ChartCollection
    {
        /// <summary>
        /// Contructor
        /// </summary>
        public ChartCollection()
        {
            OrderSeries = new ScatterSeries() { Title = "Orders", Values = new ChartValues<DateTimePoint>(), PointGeometry= DefaultGeometries.Square, Opacity=1 };
            DividendSeries = new ScatterSeries() { Title = "Dividends", Values = new ChartValues<DateTimePoint>(), PointGeometry = DefaultGeometries.Triangle, Opacity=1 };
            AbsoluteSeries = new LineSeries() { Title = "Absolute Value", Values = new ChartValues<DateTimePoint>(), PointGeometry=null };
            GrowthSeries = new LineSeries() { Title = "Growth", Values = new ChartValues<DateTimePoint>(), PointGeometry=null, ScalesYAt=1 };
        }

        /// <summary>
        /// A chart series for the <see cref="Order"/>s of a <see cref="Share"/>
        /// </summary>
        public ScatterSeries OrderSeries { get; set; }
        /// <summary>
        /// A chart series for the <see cref="Dividend"/>s of a <see cref="Share"/>
        /// </summary>
        public ScatterSeries DividendSeries { get; set; }
        /// <summary>
        /// A chart series for the <see cref="ShareValue"/>s of a <see cref="Share"/>
        /// </summary>
        public LineSeries AbsoluteSeries { get; set; }
        /// <summary>
        /// A chart series for the percentagewise growths of a <see cref="ShareValue"/>
        /// </summary>
        public LineSeries GrowthSeries { get; set; }
        
    }
}
