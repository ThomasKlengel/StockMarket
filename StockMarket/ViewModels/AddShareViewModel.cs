using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace StockMarket.ViewModels
{
    public class AddShareViewModel : ViewModelBase
    {
        #region ctors

        /// <summary>
        /// Initializes a new instance of the <see cref="AddShareViewModel"/> class.
        /// </summary>
        public AddShareViewModel()
        {
            // Orders = new ObservableCollection<OrderViewModel>();
            // DayValues = new ObservableCollection<DayValueViewModel>();
            this.AutoFillCommand = new RelayCommand(this.AutofillAsync, this.CanAutoFill);
            this.InsertCommand = new RelayCommand(this.Insert, this.CanInsert);
        }

        #endregion

        #region Properties

        private string _shareName;

        /// <summary>
        /// Gets or sets the name of the stock company.
        /// </summary>
        public string ShareName
        {
            get
            {
                return this._shareName;
            }

            set
            {
                if (this._shareName != value)
                {
                    this._shareName = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.ShareName)));
                }
            }
        }

        private string _webSite;

        /// <summary>
        /// Gets or sets the website from which to get the data for the <see cref="Share"/>.
        /// </summary>
        public string WebSite
        {
            get
            {
                return this._webSite;
            }

            set
            {
                if (this._webSite != value)
                {
                    this._webSite = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.WebSite)));
                }
            }
        }

        private string _webSite2;

        /// <summary>
        /// Gets or sets the website from which to get the data for the <see cref="Share"/>.
        /// </summary>
        public string WebSite2
        {
            get
            {
                return this._webSite2;
            }

            set
            {
                if (this._webSite2 != value)
                {
                    this._webSite2 = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.WebSite2)));
                }
            }
        }

        private string _webSite3;

        /// <summary>
        /// Gets or sets the website from which to get the data for the <see cref="Share"/>.
        /// </summary>
        public string WebSite3
        {
            get
            {
                return this._webSite3;
            }

            set
            {
                if (this._webSite3 != value)
                {
                    this._webSite3 = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.WebSite3)));
                }
            }
        }

        private string _wkn;

        /// <summary>
        /// Gets or sets the WKN of the <see cref="Share"/>.
        /// </summary>
        public string WKN
        {
            get
            {
                return this._wkn;
            }

            set
            {
                if (this._wkn != value)
                {
                    this._wkn = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.WKN)));
                }
            }
        }

        private string _isin;

        /// <summary>
        /// Gets or sets the ISIN of the <see cref="Share"/>.
        /// </summary>
        public string ISIN
        {
            get
            {
                return this._isin;
            }

            set
            {
                if (this._isin != value)
                {
                    this._isin = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.ISIN)));
                }
            }
        }

        private double _actPrice;

        /// <summary>
        /// Gets or sets the current price for the share.
        /// </summary>
        public double ActualPrice
        {
            get
            {
                return this._actPrice;
            }

            set
            {
                if (this._actPrice != value)
                {
                    this._actPrice = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.ActualPrice)));
                }
            }
        }

        private ShareType _shareType = ShareType.Share;

        public ShareType ShareType
        {
            get
            {
                return this._shareType;
            }

            set
            {
                if (this._shareType != value)
                {
                    this._shareType = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.ShareType)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.IsShare)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.IsCertificate)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.IsETF)));
                }
            }
        }

        public bool IsShare
        {
            get
            {
                return this.ShareType == ShareType.Share;
            }

            set
            {
                if (value)
                {
                    this.ShareType = ShareType.Share;
                }
            }
        }

        public bool IsCertificate
        {
            get
            {
                return this.ShareType == ShareType.Certificate;
            }

            set
            {
                if (value)
                {
                    this.ShareType = ShareType.Certificate;
                }
            }
        }

        public bool IsETF
        {
            get
            {
                return this.ShareType == ShareType.ETF;
            }

            set
            {
                if (value)
                {
                    this.ShareType = ShareType.ETF;
                }
            }
        }

        private byte _factor;

        public byte Factor
        {
            get
            {
                return this._factor;
            }

            set
            {
                if (this._factor != value)
                {
                    this._factor = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Factor)));
                }
            }
        }

        #endregion

        #region Commands
        public RelayCommand AutoFillCommand { get; private set; }

        /// <summary>
        /// The execute method of the AutoFill<see cref="RelayCommand"/>
        /// Sets <see cref="AddShareViewModel"/> properties after reading data from a website.
        /// </summary>
        /// <param name="o">A parameter for this method.</param>
        private async void AutofillAsync(object o)
        {
            // set emtpty values
            // set empty values
            string wkn = string.Empty, isin = string.Empty, name = string.Empty;
            double price = 0;

            // get the web content
            string webContent = await WebHelper.GetWebContent(this.WebSite);

            // set empty values
            RegexHelper.GetShareIDs(webContent, out name, out isin, out wkn, out price);

            // check the share type
            var type = RegexHelper.GetShareTypeShare(this.WebSite);
            switch (type)
            {
                case ShareType.Share:
                    this.IsShare = true;
                    Factor = 1;
                    break;
                case ShareType.Certificate:
                    this.IsCertificate = true;
                    break;
                case ShareType.ETF:
                    this.IsShare = false;
                    this.IsCertificate = false;
                    Factor = 1;
                    break;
            }


            if (price != 0)
            {
                // set values if it is a share
                if (this.IsShare)
                {
                    this.Factor = 1;
                    // get the current price
                    // price = RegexHelper.GetSharePrice(webContent,ShareType.Share);
                    await Task.Run(async () =>
                    {
                        price = await RegexHelper.GetSharePriceAsync(new Share(string.Empty, this.WebSite, "iah345", "de0000000000"));
                    });
                }

                // set values if it is a certificate
                if (this.IsCertificate)
                {
                    // get values of WKN and ISIN
                    var title = Regex.Match(webContent, RegexHelper.REGEX_CertificateTitle);

                    // get the certificate factor
                    var factorMatch = Regex.Match(title.Value, RegexHelper.REGEX_Group_CertFactor);
                    this.Factor = Convert.ToByte(factorMatch.Value.Substring(6));
                    // get the current bid price
                    // var priceMath = Regex.Match(webContent, RegexHelper.REGEX_Group_CertPrice);
                    // price = Convert.ToDouble(Regex.Match(priceMath.Value,RegexHelper.REGEX_SharePrice).Value);
                    await Task.Run(async () =>
                    {
                        price = await RegexHelper.GetSharePriceAsync(new Share(string.Empty, this.WebSite, "iah345", "de0000000000", ShareType.Certificate));
                    });
                    // get name of SHARE certificate
                    var nameMatch = Regex.Match(title.Value, RegexHelper.REGEX_Group_CertName);
                    name = nameMatch.Value.Substring(4).Replace(" von", string.Empty).Trim() + " Certificate " + factorMatch.Value + "x";
                }
            }
            // write values to view model
            this.ISIN = isin;
            this.ShareName = name;
            this.WKN = wkn;
            this.ActualPrice = price;

            // DayValues.Add(new DayValueViewModel() { Date = DateTime.Today, Price = ActualPrice });
        }

        /// <summary>
        /// The canExecute method of the AutoFill<see cref="RelayCommand"/>
        /// Checks by validating a website.
        /// </summary>
        /// <param name="sender">A parameter for this method.</param>
        private bool CanAutoFill(object sender)
        {
            Button b = new Button();
            if (sender != null)
            {
                if (sender.GetType() == typeof(Button))
                {
                    b = sender as Button;
                    b.ToolTip = "Website is not valid";
                }
            }

            if (this.WebSite != null)
            {
                if (RegexHelper.WebsiteIsValid(this.WebSite))
                {
                    b.ToolTip = "Website is valid";
                    return true;
                }
            }

            return false;
        }

        public RelayCommand InsertCommand { get; private set; }

        /// <summary>
        /// The execute method of the Insert<see cref="RelayCommand"/>
        /// Tries to add the share to the Database.
        /// </summary>
        /// <param name="o">A parameter for this method.</param>
        private void Insert(object o)
        {
            // Add the share to the database
            switch (DataBaseHelper.AddShareToDB(this))
            {
                case 1: this.WebSite = string.Empty; break;
                case 0: // Message if it already exist
                    MessageBox.Show($"You already have a share with an ISIN matching ISIN={this.ISIN}.");
                    break;
                case -1: // Message if the was an error while inserting
                    MessageBox.Show($"There was an error while inserting the share with the ISIN={this.ISIN} to the database.");
                    break;
                default: break;
            }
        }

        /// <summary>
        /// The canExecute method of the Insert<see cref="RelayCommand"/>
        /// Checks by validating ISIN and WebSite.
        /// </summary>
        /// <param name="sender">A parameter for this method.</param>
        private bool CanInsert(object sender)
        {

            Button b = new Button();
            if (sender != null)
            {
                // check if sender/executer is a button
                if (sender.GetType() == typeof(Button))
                {
                    b = sender as Button;
                    // set default tool tip
                    b.ToolTip = "Website or ISIN are not valid";
                }
                else
                {
                    return false;
                }
            }

            if (this.WebSite != null && this.ISIN != null)
            {
                // if WebSite is not valid...
                if (!RegexHelper.WebsiteIsValid(this.WebSite))
                {
                    // ... set error tool tip
                    b.ToolTip = "Website is not valid";
                    return false;
                }

                // if ISIN is not valid...
                else if (!RegexHelper.IsinIsValid(this.ISIN))
                {   // ... set error tool tip
                    b.ToolTip = "ISIN is not valid";
                    return false;
                }

                // if both are valid...
                else
                {   // ... set OK tool tip
                    b.ToolTip = "You can add the share to the database";
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
