using System;
using System.ComponentModel;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// ViewModel for the value of a share at a day 
    /// </summary>
    public class DayValueViewModel : ViewModelBase
    {
        #region ctors

        public DayValueViewModel() { }

        #endregion


        #region Properties
        private double _price;

        /// <summary>
        /// The price of the <see cref="Share"/> at the <see cref="Date"/>
        /// </summary>
        public double Price
        {
            get { return _price; }
            set
            {
                if (_price != value)
                {
                    _price = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Price)));
                }
            }
        }

        private DateTime _date;

        /// <summary>
        /// The Date of the <see cref="Price"/>
        /// </summary>
        public DateTime Date
        {
            get { return _date; }
            set
            {
                if (_date != value)
                {
                    _date = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Date)));
                }
            }
        }

        #endregion

    }
}
