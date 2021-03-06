﻿using Microsoft.Win32;
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
    public class AddDividendViewModel : ViewModelBase
    {

        #region ctor
        public AddDividendViewModel()
        {
            AddDividendCommand = new RelayCommand(AddDividend, CanAddDividednd);
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

        #region Fields
        /// <summary>
        /// set to true when the <see cref="Dividend"/> was changed to ignore its own refresh in the <see cref="DividendPerShare"/>
        /// </summary>
        private bool DividendChanged = false;
        /// <summary>
        /// set to true when the <see cref="DividendPerShare"/> was changed to ignore its own refresh in the <see cref="Dividend"/>
        /// </summary>
        private bool DPSChanged = false;
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
        /// The <see cref="Share"/>s the user can select to add a dividend for
        /// </summary>
        public ObservableCollection<Share> Shares { get; set; }

        private double _dividend;
        /// <summary>
        /// The whole dividend for the <see cref="Amount"/> of <see cref="SelectedShare"/>s
        /// </summary>
        public double Dividend
        {
            get { return _dividend; }
            set
            {
                if (_dividend != value)
                {
                    _dividend = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Dividend)));

                    // dont set "dividend by share" if the dividend has been changed by "dividend per share"
                    if (!DPSChanged)
                    {
                        DividendChanged = true;
                        DividendPerShare = Dividend / Amount;
                    }
                    DPSChanged = false;

                }
            }
        }


        private double _dividendPerShare;
        /// <summary>
        /// The current price of a single share
        /// </summary>
        public double DividendPerShare
        {
            get { return _dividendPerShare; }
            set
            {
                if (_dividendPerShare != value)
                {
                    _dividendPerShare = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(DividendPerShare)));

                    // refresh the dividend return
                    if (Amount != 0 && Dividend != 0.0)
                    {
                        GetDividendReturnAsync();
                    }

                    // don't set "dividend" if the dividend per share has been changed by "dividend"
                    if (!DividendChanged)
                    {
                        if (Dividend / Amount != DividendPerShare)
                        {
                            Dividend = DividendPerShare * Amount;
                            DPSChanged = true;
                        }
                    }
                    DividendChanged = false;
                }
            }
        }

        private Share _selectedShare;
        /// <summary>
        ///  The <see cref="Share"/> currently selected by the user to add a dividend to
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
                }

            }
        }

        private double _amount;
        /// <summary>
        /// The amount of shares for which the dividend was given
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
                    Dividend = DividendPerShare * Amount;
                }
            }
        }

        private DateTime _dividendPayDate = DateTime.Today;
        /// <summary>
        /// The date at which the dividend was payed
        /// </summary>
        public DateTime DividendPayDate
        {
            get { return _dividendPayDate; }
            set {
                if (_dividendPayDate != value)
                {
                    _dividendPayDate = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(DividendPayDate)));
                }
            }
        }

        private DateTime _dividendRangeStartDate = DateTime.Today.AddYears(-1);
        /// <summary>
        ///  The starting date of the time the share was held
        /// </summary>
        public DateTime DateRangeStart
        {
            get { return _dividendRangeStartDate; }
            set
            {
                if (_dividendRangeStartDate != value)
                {
                    _dividendRangeStartDate = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(DateRangeStart)));
                }
            }
        }

        private DateTime _dividendRangeEndDate = DateTime.Today;
        /// <summary>
        ///  The last date of the time the share was held
        /// </summary>
        public DateTime DateRangeEnd
        {
            get { return _dividendRangeEndDate; }
            set
            {
                if (_dividendRangeEndDate != value)
                {
                    _dividendRangeEndDate = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(DateRangeEnd)));
                }
            }
        }

        private double currentPrice;
        private double _dividendReturn;
        /// <summary>
        /// The dividend return (dividen per share/share value)
        /// </summary>
        public double DividendReturn
        {
            get
            {
                return _dividendReturn;
            }
            private set
            {
                if (_dividendReturn!=value)
                {
                    _dividendReturn = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(DividendReturn)));
                }
            }

        }
        #endregion

        #region Methods
        
        /// <summary>
        /// Sets the <see cref="DividendReturn"/> for the <see cref="SelectedShare"/>
        /// </summary>
        private async void GetDividendReturnAsync()
        {
            await Task.Run(async () =>
            {
                // get the current price of the selected share
                currentPrice = await RegexHelper.GetSharePriceAsync(SelectedShare);
                if (currentPrice == 0.0)
                {
                    DividendReturn = 0.0;
                }
                // calculate the return (dividenden rendite)
                DividendReturn = DividendPerShare / currentPrice;
            });
        }

        #endregion

        #region Commands
        public RelayCommand AddDividendCommand { get; private set; }

        public RelayCommand AddInputViaPdfCommand { get; private set; }

        private void AddDividend(object o)
        {
            if (CurrentUser.Equals(User.Default()))
            {
                System.Windows.MessageBox.Show("There is no valid user selected");
                return;
            }

            // create a new dividend
            Dividend dividend = new Dividend
            {
                ISIN = SelectedShare.ISIN,
                Amount = Amount,
                Value = Dividend,
                DayOfPayment = DividendPayDate,                
                DateRangeStart = DateRangeStart,
                DateRangeEnd = DateRangeEnd,
                UserName = CurrentUser.ToString()
            };

            // add the order to the matching share
            DataBaseHelper.AddDividendToDB(dividend);

            Amount = 0;
            Dividend = 0.0;
            DividendPerShare = 0;
            DividendReturn = 0;
        }

        private bool CanAddDividednd(object o)
        {
            return (Amount > 0 && Dividend > 0.0) ? true : false;
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

            // when a file was selected....
            if (ofd.ShowDialog() == true)
            {
                string pdfToRead = ofd.FileName;

                try
                {                
                    var document = PdfReader.PdfToText(pdfToRead, 0);
                    var lines = PdfReader.GetLinesStartingWith(document, new List<string>() { "Stück", "Ausmachender", "Bestandsstichtag", "Geschäftsjahr" });

                    string word = "Stück";

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

                    //get dividend
                    word = "Ausmachender";
                    if (lines.ContainsKey(word))
                    {
                        var line = lines[word];

                        try
                        {
                            // get share price
                            var strPrice = line.Words[2].Replace("+", "");
                            double doublePrice = 0.0;
                            Double.TryParse(strPrice, out doublePrice);
                            DividendChanged = false;
                            DPSChanged = false;
                            Dividend = doublePrice;
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Error on reading of whole dividend");
                        }
                    }

                    //get dividend pay date
                    word = "Bestandsstichtag";
                    if (lines.ContainsKey(word))
                    {
                        var line = lines[word];

                        try
                        {
                            var match = Regex.Match(line.ToString(), "\\d{2}\\.\\d{2}.\\d{4}");
                            var strDate = match.Value;
                            DateTime Date;
                            DateTime.TryParse(strDate, out Date);
                            DividendPayDate = Date;
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Error on reading of booking date");
                        }
                    }

                    //get dividend Start Range Date and End Range Date
                    word = "Geschäftsjahr";
                    if (lines.ContainsKey(word))
                    {
                        var line = lines[word];

                        try
                        {
                            var matches = Regex.Matches(line.ToString(), "\\d{2}\\.\\d{2}.\\d{4}");

                            // get date
                            var strDate = matches[0].Value;
                            DateTime Date;
                            DateTime.TryParse(strDate, out Date);
                            DateRangeStart = Date;

                            strDate = matches[1].Value;
                            DateTime.TryParse(strDate, out Date);
                            DateRangeEnd = Date;
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Error on reading dividend timespan");
                        }
                    }


                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error on reading pdf + {ex.Message}");
                }

            }
        }
        #endregion
    }
}
