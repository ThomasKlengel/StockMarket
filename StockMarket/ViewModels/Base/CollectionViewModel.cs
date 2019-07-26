using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// a base class for share related collections
    /// </summary>
    public abstract class CollectionViewModel: ViewModelBase
    {
        /// <summary>
        /// The price for the shares today
        /// </summary>
        public abstract double SumNow
        { get; }

        /// <summary>
        /// The price for the shares at the day bought
        /// </summary>
        public abstract double SumBuy
        { get; }

        /// <summary>
        /// The amount of bought shares
        /// </summary>
        public abstract int Amount
        {
            get; set;
        }

        /// <summary>
        /// The amount of sold shares
        /// </summary>
        public abstract int AmountSold
        {
            get; 
        }

        /// <summary>
        /// The price difference between buy and today
        /// </summary>
        public double Difference
        {
            get
            {
                return SumNow - SumBuy;
            }
        }

        /// <summary>
        /// The background for a share 
        /// </summary>
        public Brush Background
        {
            get
            {
                var paleRed = Color.FromRgb(255, 127, 127);
                var paleGreen = Color.FromRgb(222, 255, 209);
                var color = Percentage > 0.0 ? paleGreen : paleRed;
                // create solid background for shares thet are not completely sold
                Brush solidBack = new SolidColorBrush(color);
                // create partly gray background for shares that are completely sold
                Brush gradientBack = new LinearGradientBrush(Colors.Gray, color, 0);

                return Amount-AmountSold > 0 ? solidBack : gradientBack;
            }
        }

        /// <summary>
        /// The lost/gained percentage
        /// </summary>
        public double Percentage
        {
            get { return SumNow / SumBuy - 1.0; }
        }
    }
}
