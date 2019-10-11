using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace StockMarket.ViewModels
{
    public class ShareDetailViewModel : AddShareViewModel
    {
        #region ctors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShareDetailViewModel"/> class.
        /// </summary>
        public ShareDetailViewModel()
        {
            this.Shares = DataBaseHelper.GetSharesFromDB();
            this.SelectedShare = this.Shares.First();
            this.CopyCommand = new RelayCommand(this.Copy, this.CanCopy);
            this.ModifyShareCommand = new RelayCommand(this.ModifyShare, this.CanModifiyShare);
        }
        #endregion

        bool PropChanged = false;

        #region Properties

        /// <summary>
        /// Gets the <see cref="Share"/>s that are currently managed in the database.
        /// </summary>
        public List<Share> Shares { get; private set; }

        private Share _selectedShare;

        /// <summary>
        /// Gets or sets the <see cref="Share"/> that is currently selected.
        /// </summary>
        public Share SelectedShare
        {
            get
            {
                return this._selectedShare;
            }

            set
            {
                if (this._selectedShare != value)
                {
                    this._selectedShare = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.SelectedShare)));

                    // refresh the details
                    var share = DataBaseHelper.GetSharesFromDB().Find((s) => { return s.ISIN == this.SelectedShare.ISIN; });

                    this.WebSite = share.WebSite;
                    this.WebSite2 = share.WebSite2;
                    this.WebSite3 = share.WebSite3;
                    this.WKN = share.WKN;
                    this.ISIN = share.ISIN;
                    this.ShareName = share.ShareName;
                    this.IsCertificate = share.ShareType == ShareType.Certificate;
                    this.IsShare = share.ShareType == ShareType.Share;
                    this.Factor = 1;
                    if (this.IsCertificate)
                    {
                        var namePart = this.ShareName.Substring(this.ShareName.LastIndexOf(" "));
                        namePart = namePart.Replace("x", string.Empty);
                        this.Factor = Convert.ToByte(namePart);
                    }

                    this.PropertyChanged -= this.ShareDetailViewModel_PropertyChanged;
                    this.PropertyChanged += this.ShareDetailViewModel_PropertyChanged;
                }
            }
        }

        private void ShareDetailViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var share = DataBaseHelper.GetSharesFromDB().Find((s) => { return s.ISIN == this.SelectedShare.ISIN; });

            if (share.WebSite != this.WebSite ||
                share.WebSite2 != this.WebSite2 ||
                share.WebSite3 != this.WebSite3)
            {
                this.PropChanged = true;
            }
            else
            {
                this.PropChanged = false;
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
            this.PropChanged = false;
            var modShare = new Share() { ISIN = this.ISIN, ShareName = this.ShareName, WKN = this.WKN, WebSite = this.WebSite, WebSite2 = this.WebSite2, WebSite3 = this.WebSite3 };
            DataBaseHelper.ModifiyShare(modShare);
        }

        private bool CanModifiyShare(object input)
        {
            if (this.PropChanged)
            {
                if ((RegexHelper.WebsiteIsValid(this.WebSite) || this.WebSite.IsNullEmptyWhitespace() ) &&
                    (RegexHelper.WebsiteIsValid(this.WebSite2) || this.WebSite2.IsNullEmptyWhitespace()) &&
                    (RegexHelper.WebsiteIsValid(this.WebSite3) || this.WebSite3.IsNullEmptyWhitespace()) &&
                    (RegexHelper.WebsiteIsValid(this.WebSite) || RegexHelper.WebsiteIsValid(this.WebSite2) || RegexHelper.WebsiteIsValid(this.WebSite3))
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
