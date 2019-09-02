using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;


namespace StockMarket.ViewModels
{
    public class AddShareViewModel : ViewModelBase
    {
        #region ctors
        public AddShareViewModel()
        {
            //Orders = new ObservableCollection<OrderViewModel>();
            //DayValues = new ObservableCollection<DayValueViewModel>();

            AutoFillCommand = new RelayCommand(AutofillAsync, CanAutoFill);
            InsertCommand = new RelayCommand(Insert, CanInsert);
        }

        #endregion

        #region Properties                  

        private string _shareName;
        /// <summary>
        /// The name of the stock company
        /// </summary>
        public string ShareName
        {
            get { return _shareName; }
            set
            {
                if (_shareName != value)
                {
                    _shareName = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ShareName)));
                }
            }
        }

        private string _webSite;
        /// <summary>
        /// The website from which to get the data for the <see cref="Share"/>
        /// </summary>
        public string WebSite
        {
            get { return _webSite; }
            set
            {
                if (_webSite != value)
                {
                    _webSite = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(WebSite)));
                }
            }
        }

        private string _webSite2;
        /// <summary>
        /// The website from which to get the data for the <see cref="Share"/>
        /// </summary>
        public string WebSite2
        {
            get { return _webSite2; }
            set
            {
                if (_webSite2 != value)
                {
                    _webSite2 = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(WebSite2)));
                }
            }
        }

        private string _webSite3;
        /// <summary>
        /// The website from which to get the data for the <see cref="Share"/>
        /// </summary>
        public string WebSite3
        {
            get { return _webSite3; }
            set
            {
                if (_webSite3 != value)
                {
                    _webSite3 = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(WebSite3)));
                }
            }
        }

        private string _wkn;
        /// <summary>
        /// Thw WKN of the <see cref="Share"/>
        /// </summary>
        public string WKN
        {
            get { return _wkn; }
            set
            {
                if (_wkn != value)
                {
                    _wkn = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(WKN)));
                }
            }
        }

        private string _isin;
        /// <summary>
        /// The ISIN of the <see cref="Share"/>
        /// </summary>
        public string ISIN
        {
            get { return _isin; }
            set
            {
                if (_isin != value)
                {
                    _isin = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ISIN)));
                }
            }
        }

        private double _actPrice;
        /// <summary>
        /// The current price for the share
        /// </summary>
        public double ActualPrice
        {
            get { return _actPrice; }
            set
            {
                if (_actPrice != value)
                {
                    _actPrice = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ActualPrice)));
                }
            }
        }

        private bool _shareTypeIsShare=true;
        public bool IsShare
        {
            get { return _shareTypeIsShare; }
            set
            {
                if (value)
                {
                    _shareTypeIsShare = true;
                }
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsShare)));
            }
        }

        public bool IsCertificate
        {
            get { return !_shareTypeIsShare; }
            set
            {
                if (value)
                {
                    _shareTypeIsShare = false;
                }
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsCertificate)));
            }
        }

        private byte _factor;
        public byte Factor
        {
            get { return _factor; }
            set
            {
                if (_factor != value)
                {
                    _factor = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Factor)));
                }
            }
        }

        #endregion

        #region Commands
        public RelayCommand AutoFillCommand { get; private set; }
        
        /// <summary>
        /// The execute method of of the AutoFill<see cref="RelayCommand"/>
        /// Sets <see cref="AddShareViewModel"/> properties after reading data from a website
        /// </summary>
        /// <param name="o">A parameter for this method</param>
        private async void AutofillAsync(object o)
        {
            // get the webcontent
            string webContent = await WebHelper.getWebContent(WebSite);

            //check the share type
            if (RegexHelper.IsShareTypeShare(WebSite))
            {
                IsShare = true;
            }
            else
            {
                IsCertificate = true;
            }


            //set empty values
            string wkn = string.Empty, isin = string.Empty, name = string.Empty;
            double price = 0.0;

            // set values if it is a share
            if (IsShare)
            {
                // get values of WKN and ISIN
                var idMatch = Regex.Match(webContent, RegexHelper.REGEX_Group_IDs);
                var wknMatch = Regex.Match(idMatch.Value, RegexHelper.REGEX_WKN);
                var isinMatch = Regex.Match(idMatch.Value, RegexHelper.REGEX_ISIN);
                wkn = wknMatch.Value.Substring(5);
                isin = isinMatch.Value.Substring(6);

                //< h2 class="box-headline">Aktienkurs Infineon AG in <span id = "jsCurrencySelect" > EUR </ span >
                // get name of SHARE
                var nameMatch = Regex.Match(webContent, RegexHelper.REGEX_Group_ShareName);
                var nameM2 = Regex.Match(nameMatch.Value, RegexHelper.REGEX_ShareName);
                name = nameM2.Value.Substring(10).Trim().Replace(" in", "");
                //Factor for Shares is always 1
                Factor = 1;
                //get the current price
                //price = RegexHelper.GetSharePrice(webContent,ShareType.Share);
                price = await RegexHelper.GetSharePriceAsync(new Share(string.Empty, WebSite, "iah345", "de0000000000"));
            }


            //set values if it is a certificate
            if (IsCertificate)
            {
                // get values of WKN and ISIN
                var title = Regex.Match(webContent, RegexHelper.REGEX_CertificateTitle);
                var wknMatch = Regex.Match(title.Value, RegexHelper.REGEX_Group_CertWKN);
                var isinMatch = Regex.Match(title.Value, RegexHelper.REGEX_Group_CertISIN);
                wkn = wknMatch.Value.Replace("|","").Trim();
                isin = isinMatch.Value.Replace("|", "").Trim();
                                
                // get the certificate factor
                var factorMatch = Regex.Match(title.Value,RegexHelper.REGEX_Group_CertFactor);
                Factor = Convert.ToByte(factorMatch.Value.Substring(6));
                // get the current bid price
                //var priceMath = Regex.Match(webContent, RegexHelper.REGEX_Group_CertPrice);
                //price = Convert.ToDouble(Regex.Match(priceMath.Value,RegexHelper.REGEX_SharePrice).Value);

                price = await RegexHelper.GetSharePriceAsync(new Share(string.Empty, WebSite, "iah345", "de0000000000", ShareType.Certificate));

                // get name of SHARE certificate
                var nameMatch = Regex.Match(title.Value, RegexHelper.REGEX_Group_CertName);
                name = nameMatch.Value.Substring(4).Replace(" von", "").Trim() + " Certificate "+factorMatch.Value+"x";
            }

            // write values to viewmodel
            ISIN = isin;
            ShareName = name;
            WKN = wkn;
            ActualPrice = price;

            //DayValues.Add(new DayValueViewModel() { Date = DateTime.Today, Price = ActualPrice });

        }

        /// <summary>
        /// The canExecute method of of the AutoFill<see cref="RelayCommand"/>
        /// Checks by validating a website
        /// </summary>
        /// <param name="sender">A parameter for this method</param>
        private bool CanAutoFill(object sender)
        {
            Button b = new Button(); 
            if (sender!= null)
            {
                if (sender.GetType()==typeof(Button))
                {
                    b = sender as Button;
                    b.ToolTip = "Website is not valid";
                }
            }
            if (WebSite != null)
            {
                if (RegexHelper.WebsiteIsValid(WebSite))                
                {
                    b.ToolTip = "Website is valid";
                    return true;
                }
            }

            return false;

        }
               
        public RelayCommand InsertCommand { get; private set; }

        /// <summary>
        /// The execute method of of the Insert<see cref="RelayCommand"/>
        /// Tries to add the share to the Database
        /// </summary>
        /// <param name="o">A parameter for this method</param>
        private void Insert(object o)
        {
            // Add the share to the database
            switch (DataBaseHelper.AddShareToDB(this))
            {
                case 1: WebSite = String.Empty; break;
                case 0: // Message if it already exist
                    MessageBox.Show($"You already have a share with an ISIN matching ISIN={ISIN}.");
                    break;
                case -1: // Message if the was an error while inseting
                    MessageBox.Show($"There was an error while inserting the share with the ISIN={ISIN} to the database.");
                    break;
                default: break;
            }

        }

        /// <summary>
        /// The canExecute method of of the Insert<see cref="RelayCommand"/>
        /// Checks by validating ISIN and WebSite
        /// </summary>
        /// <param name="sender">A parameter for this method</param>
        private bool CanInsert(object sender)
        {
            
            Button b = new Button();
            if (sender != null)
            {
                // check if sender/executer is a button
                if (sender.GetType() == typeof(Button))
                {
                    b = sender as Button;
                    // set default tooltip
                    b.ToolTip = "Website or ISIN are not valid";
                }
                else
                {
                    return false;
                }
            }
            

            if (WebSite != null && ISIN != null)
            {
                // if WebSite is not valid...
                if (!RegexHelper.WebsiteIsValid(WebSite))
                {
                    //... set error tooltip
                    b.ToolTip = "Website is not valid";
                    return false;
                }
                // if ISIN is not valid...
                else if (!RegexHelper.IsinIsValid(ISIN))
                {   //... set error tooltip
                    b.ToolTip = "ISIN is not valid";
                    return false;
                }
                // if both are valid...
                else
                {   //... set ok tooltip
                    b.ToolTip = "You can add the share to the database";
                    return true;
                }                           
            }
            return false;


        }

        #endregion
    }

}
