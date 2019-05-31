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
            Orders = new ObservableCollection<OrderViewModel>();
            DayValues = new ObservableCollection<DayValueViewModel>();

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



        private ObservableCollection<OrderViewModel> _orders;
        /// <summary>
        /// A list of orders for the share
        /// </summary>
        public ObservableCollection<OrderViewModel> Orders
        {
            get { return _orders; }
            set
            {
                _orders = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Orders)));
            }
        }

        /// <summary>
        /// A list of prices of this share at specific dates
        /// </summary>
        private ObservableCollection<DayValueViewModel> _dayValues;
        public ObservableCollection<DayValueViewModel> DayValues
        {
            get { return _dayValues; }
            set
            {
                _dayValues = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(DayValues)));
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new instance of <see cref="ShareViewModel"/> for a given <see cref="Share"/>
        /// </summary>
        /// <param name="model">The datamodel containing all <see cref="Share"/>s</param>
        /// <param name="share">The <see cref="Share"/> for which to create the ViewModel</param>
        /// <returns>A new instance of <see cref="ShareViewModel"/></returns>
        public static ShareViewModel CreateFromShare(SharesDataModel model, Share share)
        {
            //creat a new instance
            ShareViewModel vm_Share = new ShareViewModel();

            //populate with data of the share
            vm_Share.ISIN = share.ISIN;
            vm_Share.WKN = share.WKN;
            vm_Share.ShareName = share.ShareName;
            vm_Share.WebSite = share.WebSite;

            // create a new ordercollection 
            var orderCollection = new ObservableCollection<OrderViewModel>();

            //select all orders with a matchin ISIN
            var orders = from o in model.Orders
                         where o.ISIN == vm_Share.ISIN
                         select o;
                        
            foreach (var order in orders)
            {   // create a new orderviemodel with the given data
                // and add it to the collection
                orderCollection.Add(new OrderViewModel()
                {
                    Date = order.Date,
                    Amount = order.Amount,
                    OrderExpenses = order.OrderExpenses,
                    OrderType = order.OrderType,
                    SharePrice = order.SharePrice
                });
            }
            vm_Share.Orders = orderCollection;

            // create a new collection of values
            var valueCollection = new ObservableCollection<DayValueViewModel>();

            //select all values with a matchin ISIN
            var values = from sv in model.ShareValues
                         where sv.ISIN == vm_Share.ISIN
                         select sv;

            foreach (var dayVal in values)
            {   // create a new dayviewmodel with the given data
                // and add it to the collection
                valueCollection.Add(new DayValueViewModel()
                {
                    Date = dayVal.Date,
                    Price = dayVal.Price
                });
            }

            // ad the collections to this shareviewmodel
            vm_Share.Orders = orderCollection;
            vm_Share.DayValues = valueCollection;

            return vm_Share;

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
            DayValues.Add(new DayValueViewModel() { Date = DateTime.Today, Price = ActualPrice });

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
