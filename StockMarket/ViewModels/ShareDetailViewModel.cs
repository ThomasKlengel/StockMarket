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
            ModifyShareCommand = new RelayCommand(ModifyShare, CanModifiyShare);
        }
        #endregion

        bool PropChanged = false;

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
                    WebSite2 = share.WebSite2;
                    WebSite3 = share.WebSite3;
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

                    this.PropertyChanged -= ShareDetailViewModel_PropertyChanged;
                    this.PropertyChanged += ShareDetailViewModel_PropertyChanged;
                }
            }
        }

        private void ShareDetailViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var share = DataBaseHelper.GetSharesFromDB().Find((s) => { return s.ISIN == SelectedShare.ISIN; });

            if (share.WebSite != WebSite ||
                share.WebSite2 != WebSite2 ||
                share.WebSite3 != WebSite3)
            {
                PropChanged = true;
            }
            else
            {
                PropChanged = false;
            }
        }


        #endregion

        #region Commands
        #region CopyCommand
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

        #region ModifyShareCommand
        public RelayCommand ModifyShareCommand { get; private set; }

        private void ModifyShare(object input)
        {
            PropChanged = false;
            var modShare = new Share() { ISIN = this.ISIN, ShareName = this.ShareName, WKN = this.WKN, WebSite = this.WebSite, WebSite2 = this.WebSite2, WebSite3 = this.WebSite3 };
            DataBaseHelper.ModifiyShare(modShare);
        }

        private bool CanModifiyShare(object input)
        { 
            if (PropChanged)
            {
                if ((RegexHelper.WebsiteIsValid(WebSite) || WebSite.IsNullEmptyWhitespace() )&&
                    (RegexHelper.WebsiteIsValid(WebSite2) || WebSite2.IsNullEmptyWhitespace()) &&
                    (RegexHelper.WebsiteIsValid(WebSite3) || WebSite3.IsNullEmptyWhitespace()) &&
                    (RegexHelper.WebsiteIsValid(WebSite) || RegexHelper.WebsiteIsValid(WebSite2) || RegexHelper.WebsiteIsValid(WebSite3))
                    )
                {
                    return true;
                }
            }
            return false;
        } 
        #endregion
        #endregion
    }

}
