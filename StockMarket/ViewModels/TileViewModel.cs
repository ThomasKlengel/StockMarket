using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarket.ViewModels
{
    class TileViewModel : ShareComponentViewModel
    {
        public TileViewModel(List<ShareComponentViewModel> svm, ShareComponentType type)
        {
            //initialise to zero
            _sumNow = 0;
            _sumBuy = 0;
            _amount = 0;
            _amountSold = 0;

            // add up
            foreach (var component in svm)
            {
                if (component.ComponentType != ShareComponentType.sell)
                {
                    _sumNow += component.SumNow;
                    _sumBuy += component.SumBuy;
                    if (component.ComponentType == ShareComponentType.buy)
                    {
                        _amount += component.Amount;
                        _amountSold += component.AmountSold;
                    }
                }
            }

            //refresh UI
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(SumBuy)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(SumNow)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Amount)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(AmountSold)));
            if (type == ShareComponentType.dividend)
            {
                ComponentType = ShareComponentType.dividend;
                Name = "Dividends";
            }
            else
            {
                ComponentType = ShareComponentType.buy;
                Name = "All";
            }
        }

        public string Name { get; private set; }

        readonly double _sumNow;
        public override double SumNow { get { return _sumNow; } }

        readonly double _sumBuy;
        public override double SumBuy { get { return _sumBuy; } }

        readonly int _amountSold;
        public override int AmountSold { get { return _amountSold; } }

        public override int Amount { get { return _amount; } }

    }
}
