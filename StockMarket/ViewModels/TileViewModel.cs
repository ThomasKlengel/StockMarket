﻿using System.Collections.Generic;
using System.ComponentModel;

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
            _percentage = 0;

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
                    if  (component.ComponentType== ShareComponentType.dividend)
                    {
                        _percentage += component.Percentage;                        
                    }
                }
            }

            if (type==ShareComponentType.dividend)
            {
                _amount = svm.Count;
                _percentage = _percentage / svm.Count;
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

        readonly double _amountSold;
        public override double AmountSold { get { return _amountSold; } }

        public override double Amount { get { return _amount; } }

        private readonly double _percentage;
        public override double Percentage
        {
            get
            {
                if (ComponentType == ShareComponentType.dividend)
                {
                    return _percentage;
                }
                else
                {
                    return base.Percentage;
                }
            }
        }

    }
}