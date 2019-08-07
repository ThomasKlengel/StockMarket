using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;


namespace StockMarket.ViewModels
{
    public class ShareDetailViewModel : ShareViewModel
    {
        #region ctors
        public ShareDetailViewModel()
        {
            Shares = DataBaseHelper.GetSharesFromDB();
            SelectedShare = Shares.First();
            CopyCommand = new RelayCommand(Copy, CanCopy);
        }
        #endregion


        #region Properties
        /// <summary>
        /// The <see cref="Share"/>s that are currently managed in the database
        /// </summary>
        public List<Share> Shares { get; private set; }

        private Share _selectedShare;
        /// <summary>
        /// The <see cref="Share"/> that is currently selected
        /// </summary>
        public Share SelectedShare
        {
            get { return _selectedShare; }
            set
            {
                if (_selectedShare != value)
                {
                    _selectedShare = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedShare)));

                    // refresh the details

                    var share = DataBaseHelper.GetSharesFromDB().Find((s) => { return s.ISIN == SelectedShare.ISIN; });

                    WebSite = share.WebSite;
                    WKN = share.WKN;
                    ISIN = share.ISIN;
                    ShareName = share.ShareName;
                    IsCertificate = share.ShareType == ShareType.Certificate;
                    IsShare = share.ShareType == ShareType.Share;
                    Factor = 1;
                    if (IsCertificate)
                    {
                        var namePart = ShareName.Substring(ShareName.LastIndexOf(" "));
                        namePart = namePart.Replace("x", "");
                        Factor = Convert.ToByte(namePart);
                    }
                }
            }
        }


        #endregion

        #region Commands
        public RelayCommand CopyCommand { get; private set; }

        private void Copy(object input)
        {
            if (input != null)
            {
                string text = input.ToString();
                Clipboard.SetText(text);
            }
        }

        private bool CanCopy(object input)
        {
            if (input != null)
            {
                string copyFrom = input.ToString();
                return copyFrom != string.Empty;
            }
            return false;
        }

        #endregion
    }

}
