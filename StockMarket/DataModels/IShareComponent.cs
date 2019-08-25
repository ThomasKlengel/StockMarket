using System;

namespace StockMarket.DataModels
{
    /// <summary>
    /// An interface represening <see cref="Dividend"/>s and <see cref="Order"/>s 
    /// </summary>
    public interface IShareComponent
    {
        // TODO: use interface in Dividend, Order, (CollectionViewModel?)

        int Amount { get; }
        double Percentage { get; }
        double SumNow { get; }
        double SumBuy { get; }
        DateTime BookingDate { get; }
        double SinglePriceBuy {get; }
        double SinglePriceNow { get; }
        OrderType ComponentType { get; }
    }
}
