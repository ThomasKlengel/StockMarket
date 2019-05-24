using System.Collections.ObjectModel;
using StockMarket.DataModels;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel containing information about all <see cref="Share"/>s, <see cref="Order"/>s and <see cref="ShareValue"/>s in the database
    /// </summary>
    public class MainViewModel:ViewModelBase
    {
        #region ctors
        public MainViewModel()
        {
            Shares = new ObservableCollection<ShareViewModel>();
        }

        #endregion

        #region Properties
        private ObservableCollection<ShareViewModel> _shares;

        /// <summary>
        /// A Collection of <see cref="Share"/>s represented by a <see cref="ShareViewModel"/>
        /// </summary>
        public ObservableCollection<ShareViewModel> Shares
        {
            get { return _shares; }
            set { _shares = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new <see cref="MainViewModel"/> from a <see cref="SharesDataModel"/>
        /// </summary>
        /// <param name="model">The <see cref="SharesDataModel"/> to create the see <see cref="MainViewModel"/> from</param>
        /// <returns>A new <see cref="MainViewModel"/></returns>
        public static MainViewModel PopulateFromModel(SharesDataModel model)
        {
            // create a new instance of the viewmodel
            MainViewModel vm = new MainViewModel();

            // fill it with data of the shares
            foreach (var share in model.Shares)
            {
                vm.Shares.Add(ShareViewModel.CreateFromShare(model,share));
            }
            return vm;
        }

        #endregion
    }
}
