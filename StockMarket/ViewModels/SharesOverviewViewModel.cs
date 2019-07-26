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
    class SharesOverviewViewModel : CollectionViewModel
    {
        #region ctor
        public SharesOverviewViewModel()
        {
            Shares = new List<ShareOverviewViewModel>();
            var shares = DataBaseHelper.GetSharesFromDB();
            foreach (var share in shares)
            {
                ShareOverviewViewModel svm = new ShareOverviewViewModel(share);
                Shares.Add(svm);
                svm.PropertyChanged += Share_RelevantPropertyChanged;
            }
        }
        #endregion

        #region Eventhandler
        private void Share_RelevantPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SumNow":
                    {
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(SumNow)));
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Difference)));
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Percentage)));
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Background)));
                        break;
                    }
                case "SumBuy":
                    {
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(SumBuy)));
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Difference)));
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Percentage)));
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Background)));
                        break;
                    }
                default: break;
            }
        }
        #endregion

        #region Properties
        public List<ShareOverviewViewModel> Shares { get; private set; }

        /// <summary>
        /// The summed up price for all orders on the date of purchase
        /// </summary>
        override public double SumBuy
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
        override public double SumNow
        {
            get
            {
                double now = 0;

                foreach (var share in Shares)
                {
                    now += share.SumNow;
                }
                return now;
            }
        }

        override public int Amount
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
            set { return; }
        }

        public override int AmountSold
        {
            get
            {
                int amount = 0;
                foreach (var share in Shares)
                {
                    amount += share.AmountSold;
                }

                return amount;
            }
        }

        #endregion
    }

}
