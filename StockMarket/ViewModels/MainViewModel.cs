using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using StockMarket.DataModels;

namespace StockMarket.ViewModels
{
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

        public ObservableCollection<ShareViewModel> Shares
        {
            get { return _shares; }
            set { _shares = value; }
        }

        #endregion

        #region Methods

        public static MainViewModel PopulateFromModel(SharesDataModel model)
        {
            MainViewModel vm = new MainViewModel();
            foreach (var share in model.Shares)
            {
                vm.Shares.Add(ShareViewModel.CreateFromShare(model,share));
            }
            return vm;
        }

        #endregion
    }
}
