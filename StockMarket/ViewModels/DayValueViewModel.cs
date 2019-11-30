using System;
using System.ComponentModel;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// ViewModel for the value of a share at a day.
    /// </summary>
    public class DayValueViewModel : ViewModelBase
    {
        #region ctors

        /// <summary>
        /// Initializes a new instance of the <see cref="DayValueViewModel"/> class.
        /// </summary>
        public DayValueViewModel()
        {
        }

        #endregion

        #region Properties
        private double _price;

        /// <summary>
        /// Gets or sets the price of the <see cref="Share"/> at the <see cref="Date"/>.
        /// </summary>
        public double Price
        {
            get
            {
                return this._price;
            }

            set
            {
                if (this._price != value)
                {
                    this._price = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Price)));
                }
            }
        }

        private DateTime _date;

        /// <summary>
        /// Gets or sets the Date of the <see cref="Price"/>.
        /// </summary>
        public DateTime Date
        {
            get
            {
                return this._date;
            }

            set
            {
                if (this._date != value)
                {
                    this._date = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Date)));
                }
            }
        }

        #endregion

    }
}
