using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace StockMarket.ViewModels
{
    public abstract class CollectionViewModel: ViewModelBase
    {
        public abstract double SumNow
        { get; }

        public abstract double SumBuy
        { get; }

        public abstract int Amount
        {
            get; set;
        }

        public abstract int AmountSold
        {
            get; 
        }

        public double Difference
        {
            get
            {
                return SumNow - SumBuy;
            }
        }

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

        public double Percentage
        {
            get { return SumNow / SumBuy - 1.0; }
        }
    }
}
