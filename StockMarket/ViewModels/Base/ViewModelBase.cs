using System.ComponentModel;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// The basic ViewModel for simplifying the creation of other ViewModels
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}
