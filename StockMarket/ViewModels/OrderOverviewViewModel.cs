using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace StockMarket.ViewModels
{
    class OrderOverviewViewModel:ViewModelBase
    {
        #region Properties

        public double AvgSharePrice
        {
            get
            {
                double sum = 0;
                foreach (var order in Orders)
                {
                    sum += order.SharePrice;
                };
                return sum / Orders.Count;
            }

        }

        private double _actPrice;

        public double ActPrice
        {
            get { return _actPrice; }
            set
            {
                _actPrice = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ActPrice)));
            }
        }

        public int Amount
        {
            get
            {
                int sum = 0;
                foreach (var order in Orders)
                {
                    sum += order.Amount;
                };
                return sum;
            }
        }

        public DateTime Date
        {
            get { return DateTime.Today; }            
        }

        public double SumBuy
        {
            get
            {
                double sum = 0;
                foreach (var order in Orders)
                {
                    sum += order.Amount * order.SharePrice;
                };
                return sum;
            }
        }

        public double SumNow
        {
            get
            {
                double sum = 0;
                foreach (var order in Orders)
                {
                    sum += order.Amount * ActPrice;
                };
                return sum;
            }
        }

        public SolidColorBrush Backgropund
        {
            get { return SumNow - SumBuy > 0 ? new SolidColorBrush(Color.FromRgb(222, 255, 209)) : new SolidColorBrush(Color.FromRgb(255, 127, 127)); }
        }

        public double Percentage
        {
            get { return SumNow / SumBuy - 1.0; }
        }

        public List<OrderViewModel> Orders { get; set; }

        #endregion


    }
}
