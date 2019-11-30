using IronOcr;
using Microsoft.Win32;
using Prism.Events;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for a single <see cref="Order"/>
    /// </summary>
    public class AddOrderViewModel : ViewModelBase
    {
        /// <summary>
        /// set to true when a <see cref="Share"/> is selected from reading a pdf
        /// to irgnore auto updates for <see cref="ActPrice"/>, etc.
        /// </summary>
        bool ignoreUpdate = false;

        #region ctor
        public AddOrderViewModel()
        {
            AddOrderCommand = new RelayCommand(AddOrder, CanAddOrder);
            AddInputViaPdfCommand = new RelayCommand(AddInputViaPdf);
            Shares = new ObservableCollection<Share>();
            foreach (var share in  DataBaseHelper.GetSharesFromDB() )
            {
                Shares.Add(share);
            }
            SelectedShare = Shares.First();

            ApplicationService.Instance.EventAggregator.GetEvent<UserChangedEvent>().Subscribe((user) => { CurrentUser = user; });
        }

        #endregion

        #region Properties

        private User _currentUser;
        /// <summary>
        /// The <see cref="User"/> currently selected in the main window
        /// </summary>
        public User CurrentUser
        {
            get
            {
                if (_currentUser != null)
                {
                    return _currentUser;
                }
                return User.Default();
            }
            set
            {
                if (CurrentUser != value)
                {
                    _currentUser = value;
                }
            }
        }

        /// <summary>
        /// The <see cref="Share"/>s the user can choose from to add an orders to
        /// </summary>
        public ObservableCollection<Share> Shares { get; set; }

        private double _actPrice;
        /// <summary>
        /// The current price of a single share
        /// </summary>
        public double ActPrice
        {
            get { return _actPrice; }
            set
            {
                if (_actPrice != value)
                {
                    _actPrice = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ActPrice)));
                }
            }
        }

        private double _expenses =10.0;
        /// <summary>
        /// The expenses for any transaction
        /// </summary>
        public double Expenses
        {
            get { return _expenses; }
            set
            {
                if (_expenses != value)
                {
                    _expenses = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Expenses)));
                }
            }
        }

        private Share _selectedShare;
        /// <summary>
        /// The <see cref="Share"/> that is currently selected in the UI to add the orders to
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
                    // update when Share is selected by user, not when reading from PDF
                    if (!ignoreUpdate)
                    {
                        GetPriceAsync();
                        Expenses = 10;
                        Amount = 0;
                        OrderDate = DateTime.Today;
                        OrderType = ShareComponentType.buy;
                    }
                    ignoreUpdate = false;
                }
                
            }
        }
               
        private double _amount;
        /// <summary>
        /// The amount of shares purchased
        /// </summary>
        public double Amount
        {
            get { return _amount; }
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Amount)));
                }
            }
        }             

        private ShareComponentType _orderType = ShareComponentType.buy;
        /// <summary>
        /// The type of order
        /// </summary>
        public ShareComponentType OrderType
        {
            get { return _orderType; }
            set
            {
                if (_orderType != value)
                {
                    _orderType = value;
                    // Update the Checkboxes
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(OrderType)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(OrderIsBuy)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(OrderIsSell)));
                }
            }
        }
        
        /// <summary>
        /// Used to check the buy/sell-checkbox
        /// </summary>
        public bool OrderIsBuy
        {
            get
            {
                return OrderType == ShareComponentType.buy;
            }
            set
            {
                if (value) // dont update when bound CheckBox is unchecked
                {
                    OrderType = OrderType == ShareComponentType.sell ? ShareComponentType.buy : ShareComponentType.sell;
                }
            }
        }
        /// <summary>
        /// Used to check the buy/sell-checkbox
        /// </summary>
        public bool OrderIsSell
        {
            get
            {
                return OrderType == ShareComponentType.sell;
            }
            set
            {
                if (value) // dont update when bound CheckBox is unchecked
                {
                    OrderType = OrderType == ShareComponentType.sell ? ShareComponentType.buy : ShareComponentType.sell;
                }
            }
        }

        private DateTime _dateTime = DateTime.Today;
        /// <summary>
        /// The date at which the order has been booked
        /// </summary>
        public DateTime OrderDate
        {
            get { return _dateTime; }
            set {
                if (_dateTime != value)
                {
                    _dateTime = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(OrderDate)));
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the current price of the <see cref="SelectedShare"/>
        /// </summary>
        private async void GetPriceAsync()
        {
            ActPrice = await RegexHelper.GetSharePriceAsync(SelectedShare);
        }

        #endregion

        #region Commands
        public RelayCommand AddOrderCommand { get; private set; }

        public RelayCommand AddInputViaPdfCommand { get; private set; }

        private void AddOrder(object o)
        {
            // cehck if a valid user is selected
            if (CurrentUser.Equals(User.Default()))
            {
                System.Windows.MessageBox.Show("There is no valid user selected");
                return;
            }

            // create a new order
            Order order = new Order();
            order.Amount = Amount;
            order.OrderExpenses = Expenses;
            order.OrderType = OrderType;
            order.SharePrice = ActPrice;
            order.Date = OrderDate;
            order.ISIN = SelectedShare.ISIN;
            order.UserName = CurrentUser.ToString();

            // add the order to the matching share
            DataBaseHelper.AddOrderToDB(order);

            // set Amount to zero so that it cant be added by accident twice
            Amount = 0;
        }

        private bool CanAddOrder(object o)
        {
            // anly add if the amount of ordered Shares is more then zero
            return Amount > 0 ? true : false;
        }

        private void AddInputViaPdf(object o)
        {
            //create the OCR reader
            AdvancedOcr Ocr = new AdvancedOcr()
            {
                CleanBackgroundNoise = false,
                ColorDepth = 0,
                ColorSpace = AdvancedOcr.OcrColorSpace.GrayScale,
                EnhanceContrast = false,
                DetectWhiteTextOnDarkBackgrounds = false,
                RotateAndStraighten = false,
                Language = IronOcr.Languages.German.OcrLanguagePack,
                EnhanceResolution = false,
                InputImageType = AdvancedOcr.InputTypes.Document,
                ReadBarCodes = false,
                Strategy = AdvancedOcr.OcrStrategy.Advanced
            };

            // create a file dialog
            OpenFileDialog ofd = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                Filter = "PDFs|*.pdf",
                InitialDirectory = @"C:\"
            };

            if (ofd.ShowDialog() == true)
            {                
                var pdfToRead = ofd.FileName;

                // create a rectangle from which to read (dont set for complete page)
                //System.Drawing.Rectangle area = new System.Drawing.Rectangle(0, 1000, 2400, 1500);                
                var Results = Ocr.ReadPdf(pdfToRead, 1);                
                var lines = Results.Pages[0].LinesOfText.ToList();

                int i = 0;
                //get order type
                foreach (var line in lines)
                {
                    if (line.Text.StartsWith("Wertpapier Abrechnung"))
                    {
                        var buySell = line.Words.Last().Text;
                        OrderType = buySell == "Verkauf" ? ShareComponentType.sell : ShareComponentType.buy;
                        break;
                    }
                    i++;
                }
                lines.RemoveRange(0, ++i);
                i = 0;

                
                // get Amount, ISIN, WKN
                foreach (var line in lines)
                {
                    if (line.Text.StartsWith("Stück"))
                    {
                        // get ordered amount
                        var strAmount = line.Words[1].Text;
                        int intAmount = 0;
                        Int32.TryParse(strAmount, out intAmount);
                        Amount = intAmount;

                        // Share by ISIN or WKN
                        var isin = line.Words[(line.WordCount - 2)].Text;
                        var wkn = line.Words.Last().Text.Replace("(", "").Replace(")", "");

                        ignoreUpdate = true;
                        var sharesByIsin = (DataBaseHelper.GetSharesFromDB().Where((s) => { return s.ISIN == isin; }));  
                        if (sharesByIsin.Count() != 0)
                        {
                            SelectedShare = sharesByIsin.First();
                            break;
                        }
                        var sharesByWkn = (DataBaseHelper.GetSharesFromDB().Where((s) => { return s.WKN == wkn; }));
                        if (sharesByWkn.Count() != 0)
                        {
                            SelectedShare = sharesByWkn.First();
                            break;
                        }
                        var sharesByIsin0 = (DataBaseHelper.GetSharesFromDB().Where((s) => { return s.ISIN == isin.Replace("O", "0"); }));
                        if (sharesByIsin0.Count() != 0)
                        {
                            SelectedShare = sharesByIsin0.First();
                            break;
                        }
                        var sharesByWkn0 = (DataBaseHelper.GetSharesFromDB().Where((s) => { return s.WKN == wkn.Replace("O", "0"); }));
                        if (sharesByWkn0.Count() != 0)
                        {
                            SelectedShare = sharesByWkn0.First();
                            break;
                        }
                        break;
                    }
                    i++;
                }
                lines.RemoveRange(0, ++i);
                i = 0;

                //get orderdate
                foreach (var line in lines)
                {
                    if (line.Text.StartsWith("Schluss"))
                    {

                        var match = Regex.Match(line.Text, "\\d{2}\\.\\d{2}.\\d{4}");
                        // get share price
                        var strDate = match.Value;
                        DateTime Date;
                        DateTime.TryParse(strDate, out Date);
                        OrderDate = Date;
                        break;
                    }
                    i++;
                }
                lines.RemoveRange(0, ++i);
                i = 0;

                //get SharePrice at ordertime
                foreach (var line in lines)
                {
                    if (line.Text.StartsWith("Ausführungskurs"))
                    {
                        // get share price
                        var strPrice = line.Words[1].Text;
                        double doublePrice = 0.0;
                        Double.TryParse(strPrice, out doublePrice);
                        ActPrice = doublePrice;
                        break;
                    }
                    i++;
                }
                lines.RemoveRange(0, ++i);
                
                // get expenses
                int lineIndex = 0;
                int provisionLineIndex = 1000;
                foreach (var line in lines)                {
                    
                    if (line.Text.StartsWith("Provision"))
                    {
                        provisionLineIndex = lineIndex;
                        // get order expenses
                        var strExpense = line.Words[1].Text.Replace("-","");
                        double doubleExpense = 0.0;
                        Double.TryParse(strExpense, out doubleExpense);
                        Expenses = doubleExpense;
                        lineIndex++;
                        continue;
                    }

                    if (provisionLineIndex<100)
                    {
                        if (line.Text.StartsWith("Ausmachender") || line.Text.StartsWith("Ermittlung"))
                        {
                            break;
                        }

                        if (line.Words[line.Words.Count()-2].Text.EndsWith("-"))
                        {
                            // get additional expenses
                            var strExpense = line.Words[line.Words.Count() - 2].Text.Replace("-","");
                            double doubleExpense = 0.0;
                            Double.TryParse(strExpense, out doubleExpense);
                            Expenses += doubleExpense;
                        }

                    }
                    lineIndex++;
                }

            }
        }
        #endregion
    }
}
