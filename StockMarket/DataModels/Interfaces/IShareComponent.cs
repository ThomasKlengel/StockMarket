using System;

namespace StockMarket
{
    /// <summary>
    /// An interface represening <see cref="Dividend"/>s and <see cref="Order"/>s.
    /// </summary>
    public interface IShareComponent
    {
        double Amount { get; }

        double Percentage { get; }

        double SumNow { get; }

        double SumBuy { get; }

        DateTime BookingDate { get; }

        double SinglePriceBuy {get; }

        double SinglePriceNow { get; }

        ShareComponentType ComponentType { get; }
    }
}
