using System.ComponentModel;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// The basic ViewModel for simplifying the creation of other ViewModels.
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}
