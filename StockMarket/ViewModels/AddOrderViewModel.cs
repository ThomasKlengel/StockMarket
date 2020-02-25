
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for a single <see cref="Order"/>
    /// </summary>
    public class AddOrderViewModel : ViewModelBase
    {
        /// <summary>
        /// set to true when a <see cref="Share"/> is selected from reading a PDF
        /// to ignore auto updates for <see cref="ActPrice"/>, etc.
        /// </summary>
        bool ignoreUpdate = false;

        #region ctor
        public AddOrderViewModel()
        {
            AddOrderCommand = new RelayCommand(AddOrder, CanAddOrder);
            AddInputViaPdfCommand = new RelayCommand(AddInputViaPdf);
            Shares = new ObservableCollection<Share>();
            foreach (var share in DataBaseHelper.GetSharesFromDB())
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

        private double _expenses = 10.0;
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
                        OrderType = ShareComponentType.Buy;
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

        private ShareComponentType _orderType = ShareComponentType.Buy;
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
                    // Update the Check boxes
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(OrderType)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(OrderIsBuy)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(OrderIsSell)));
                }
            }
        }

        /// <summary>
        /// Used to check the buy/sell-check box
        /// </summary>
        public bool OrderIsBuy
        {
            get
            {
                return OrderType == ShareComponentType.Buy;
            }
            set
            {
                if (value) // don't update when bound CheckBox is unchecked
                {
                    OrderType = OrderType == ShareComponentType.Sell ? ShareComponentType.Buy : ShareComponentType.Sell;
                }
            }
        }
        /// <summary>
        /// Used to check the buy/sell-check box
        /// </summary>
        public bool OrderIsSell
        {
            get
            {
                return OrderType == ShareComponentType.Sell;
            }
            set
            {
                if (value) // don't update when bound CheckBox is unchecked
                {
                    OrderType = OrderType == ShareComponentType.Sell ? ShareComponentType.Buy : ShareComponentType.Sell;
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
            set
            {
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
            await Task.Run(async () =>
            {
                ActPrice = await RegexHelper.GetSharePriceAsync(SelectedShare);
            });
        }

        #endregion

        #region Commands
        public RelayCommand AddOrderCommand { get; private set; }

        public RelayCommand AddInputViaPdfCommand { get; private set; }

        private void AddOrder(object o)
        {
            // check if a valid user is selected
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
            // only add if the amount of ordered Shares is more then zero
            return Amount > 0 ? true : false;
        }

        private void AddInputViaPdf(object o)
        {
            // create a file dialog
            OpenFileDialog ofd = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                Filter = "PDFs|*.pdf",
                InitialDirectory = App.DEFAULTPATH
            };

            if (ofd.ShowDialog() == true)
            {
                ignoreUpdate = true;
                var pdfToRead = ofd.FileName;

                var document = PdfReader.PdfToText(pdfToRead, 0);
                var lines = PdfReader.GetLinesStartingWith(document, new List<string>() { "Wertpapier", "Stück", "Schluss", "Ausführungskurs", "Geschäftsjahr" });

                try
                {
                    string word = "Wertpapier";
                    //get order type
                    if (lines.ContainsKey(word))
                    {
                        var line = lines[word];
                        var buySell = line.Words.Last();
                        OrderType = buySell == "Verkauf" ? ShareComponentType.Sell : ShareComponentType.Buy;
                    }

                    word = "Stück";
                    //get ISIN, WKN, Amiount
                    if (lines.ContainsKey(word))
                    {
                        try
                        {
                            var line = lines[word];

                            // get ordered amount
                            var strAmount = line.Words[1];
                            double doubleAmount = 0;
                            Double.TryParse(strAmount, out doubleAmount);
                            Amount = doubleAmount;

                            //            // Share by ISIN or WKN
                            var isin = line.Words[(line.Words.Count - 2)];
                            var wkn = line.Words.Last().Replace("(", "").Replace(")", "");

                            var share = DataBaseHelper.GetShareByIdentifier(isin);
                            if (share != null)
                            {
                                SelectedShare = share;
                            }
                            else
                            {
                                share = DataBaseHelper.GetShareByIdentifier(wkn);
                                if (share != null)
                                {
                                    SelectedShare = share;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Error on reading of ISIN, WKN or Amount");
                        }
                    }

                    word = "Schluss";
                    //get order date
                    if (lines.ContainsKey(word))
                    {
                        var line = lines[word];
                        var match = Regex.Match(line.ToString(), "\\d{2}\\.\\d{2}.\\d{4}");
                        // get share price
                        var strDate = match.Value;
                        DateTime Date;
                        DateTime.TryParse(strDate, out Date);
                        OrderDate = Date;
                    }

                    word = "Ausführungskurs";
                    //get SharePrice at order time
                    if (lines.ContainsKey(word))
                    {
                        var line = lines[word];
                        // get share price
                        var strPrice = line.Words[1].Replace(".", "");
                        double doublePrice = 0.0;
                        Double.TryParse(strPrice, out doublePrice);
                        ActPrice = doublePrice;
                    }


                    // get expenses
                    var l1 = PdfReader.GetLinesBetween(document, "Provision", "Ermittlung");
                    l1.Remove(l1.Last());
                    var l2 = PdfReader.GetLinesBetween(document, "Provision", "Ausmachender");
                    l2.Remove(l2.Last());
                    var linesProv = l2.Count > l1.Count ? l1 : l2;

                    foreach (var line in linesProv)
                    {
                        if (line.ToString().StartsWith("Provision"))
                        {
                            // get order expenses
                            var strExpense = line.Words[1].Replace("-", "");
                            double doubleExpense = 0.0;
                            Double.TryParse(strExpense, out doubleExpense);
                            Expenses = doubleExpense;
                            continue;
                        }

                        if (line.Words[line.Words.Count() - 2].EndsWith("-"))
                        {
                            // get additional expenses
                            var strExpense = line.Words[line.Words.Count() - 2].Replace("-", "");
                            double doubleExpense = 0.0;
                            Double.TryParse(strExpense, out doubleExpense);
                            Expenses += doubleExpense;
                        }
                    }
                }
                catch (Exception ex)
                {
                    //TODO: log ex
                }

            }
        }
        #endregion
    }
}
