using System;
using System.ComponentModel;

namespace StockMarket.ViewModels
{
    public class DayValueViewModel : ViewModelBase
    {
        #region ctors

        public DayValueViewModel() { }

        #endregion


        #region Properties
        private double _price;

        public double Price
        {
            get { return _price; }
            set
            {
                _price = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Price)));
            }
        }

        private DateTime _date;

        public DateTime Date
        {
            get { return _date; }
            set
            {
                _date = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Date)));
            }
        }

        #endregion

    }
}
