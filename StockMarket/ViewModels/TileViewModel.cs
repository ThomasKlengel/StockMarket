using System.Collections.Generic;
using System.ComponentModel;

namespace StockMarket.ViewModels
{
    class TileViewModel : ShareComponentViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TileViewModel"/> class.
        /// </summary>
        /// <param name="svm"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        public TileViewModel(List<ShareComponentViewModel> svm, ShareComponentType type, string name)
        {
            // initialise to zero
            this.Name = name;
            this._sumNow = 0;
            this._sumBuy = 0;
            this._amount = 0;
            this._amountSold = 0;
            this._percentage = 0;

            // add up
            foreach (var component in svm)
            {
                if (component.ComponentType != ShareComponentType.Sell)
                {
                    this._sumNow += component.SumNow;
                    this._sumBuy += component.SumBuy;
                    if (component.ComponentType == ShareComponentType.Buy)
                    {
                        this._amount += component.Amount;
                        this._amountSold += component.AmountSold;
                    }

                    if  (component.ComponentType == ShareComponentType.Dividend)
                    {
                        this._percentage += component.Percentage;
                    }
                }
            }

            if (type == ShareComponentType.Dividend)
            {
                this._amount = svm.Count;
                this._percentage = this._percentage / svm.Count;
            }

            // refresh UI
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.SumBuy)));
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.SumNow)));
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Amount)));
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.AmountSold)));
        }

        public string Name { get; private set; }

        readonly double _sumNow;

        /// <inheritdoc/>
        public override double SumNow
        {
            get { return this._sumNow; }
        }

        readonly double _sumBuy;

        /// <inheritdoc/>
        public override double SumBuy
        {
            get { return this._sumBuy; }
        }

        readonly double _amountSold;

        /// <inheritdoc/>
        public override double AmountSold
        {
            get { return this._amountSold; }
        }

        /// <inheritdoc/>
        public override double Amount
        {
            get { return this._amount; }
        }

        private readonly double _percentage;

        /// <inheritdoc/>
        public override double Percentage
        {
            get
            {
                if (this.ComponentType == ShareComponentType.Dividend)
                {
                    return this._percentage;
                }
                else
                {
                    return base.Percentage;
                }
            }
        }
    }
}
