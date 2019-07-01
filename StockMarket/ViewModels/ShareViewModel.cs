using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using StockMarket.DataModels;


namespace StockMarket.ViewModels
{
    public class ShareViewModel : ViewModelBase
    {
        #region ctors
        public ShareViewModel()
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
                _shareName = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ShareName)));
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
                _webSite = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(WebSite)));
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
                _wkn = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(WKN)));
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
                _isin = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ISIN)));
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
                _actPrice = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ActualPrice)));
            }
        }

        private bool _shareTypeIsShare;
        public bool IsShare
        {
            get { return _shareTypeIsShare; }
            set
            {
                if (value)
                {
                    _shareTypeIsShare = _shareTypeIsShare ? !_shareTypeIsShare : true;
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
                    _shareTypeIsShare = _shareTypeIsShare ? !_shareTypeIsShare : true;
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
        /// Sets <see cref="ShareViewModel"/> properties after reading data from a website
        /// </summary>
        /// <param name="o">A parameter for this method</param>
        private async void AutofillAsync(object o)
        {
            string webContent = await WebHelper.getWebContent(WebSite);

            //id">WKN: 623100 / ISIN: DE0006231004</span>
            //var test= "nbsp;<span class=\"instrument - id\">WKN: 623100 / ISIN: DE0006231004</span></h1><div"
            // get values of WKN and ISIN
            var idMatch = Regex.Match(webContent, RegexHelper.REGEX_Group_IDs);
            var wknMatch = Regex.Match(idMatch.Value, RegexHelper.REGEX_WKN);
            var isinMatch = Regex.Match(idMatch.Value, RegexHelper.REGEX_ISIN);
            string wkn = wknMatch.Value.Substring(5);
            string isin = isinMatch.Value.Substring(6);

            //< h2 class="box-headline">Aktienkurs Infineon AG in <span id = "jsCurrencySelect" > EUR </ span >
            // get name of SHARE
            var nameMatch = Regex.Match(webContent, RegexHelper.REGEX_Group_ShareName);
            var nameM2 = Regex.Match(nameMatch.Value, RegexHelper.REGEX_ShareName);
            var name = nameM2.Value.Substring(10).Trim().Replace(" in", "");

            // set values of the share
            ISIN = isin;
            ShareName = name;
            WKN = wkn;
            ActualPrice = RegexHelper.GetSharPrice(webContent);
            IsShare = RegexHelper.IsShareTypeShare(WebSite);
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
