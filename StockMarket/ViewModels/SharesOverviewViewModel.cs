using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.Linq;
using System.Windows.Threading;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for all <see cref="Shares"/> in the <see cref="Pages.SharesOverviewPage"/>
    /// </summary>
    class SharesOverviewViewModel : ViewModelBase
    {

        public SharesOverviewViewModel()
        {
            Shares = new List<ShareOverviewViewModel>();
            var shares = DataBaseHelper.GetSharesFromDB();
            foreach (var share in shares)
            {
                Shares.Add(new ShareOverviewViewModel(share));
            }
        }

        public List<ShareOverviewViewModel> Shares { get; private set; }

        /// <summary>
        /// The summed up price for all orders on the date of purchase
        /// </summary>
        public double SumBuy
        {
            get
            {
                double buy = 0;

                foreach (var share in Shares)
                {
                    buy += share.SumBuy;
                }
                return buy;
            }
        }

        /// <summary>
        /// The current summed up price for all orders 
        /// </summary>
        public double SumNow
        {
            get
            {
                double buy = 0;

                foreach (var share in Shares)
                {
                    buy += share.SumNow;
                }
                return buy;
            }
        }

        public int Amount
        {
            get
            {
                int amount = 0;
                foreach (var share in Shares)
                {
                    amount += share.Amount;
                }

                return amount;
            }
        }

        /// <summary>
        /// The background color for the overview determined by 
        /// a positive or negative development of share prices
        /// </summary>
        public SolidColorBrush Backgropund
        {
            get
            {
                return SumNow - SumBuy > 0 ? new SolidColorBrush(Color.FromRgb(222, 255, 209))
                                           : new SolidColorBrush(Color.FromRgb(255, 127, 127));
            }
        }

        /// <summary>
        /// The development of share prices in percent
        /// </summary>
        public double Percentage
        {
            get { return SumNow / SumBuy - 1.0; }
        }
    }

}
