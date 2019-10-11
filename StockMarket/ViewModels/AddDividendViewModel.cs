using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using IronOcr;
using Microsoft.Win32;
using Prism.Events;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for a single <see cref="Order"/>.
    /// </summary>
    public class AddDividendViewModel : ViewModelBase
    {

        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AddDividendViewModel"/> class.
        /// </summary>
        public AddDividendViewModel()
        {
            this.AddDividendCommand = new RelayCommand(this.AddDividend, this.CanAddDividednd);
            this.AddInputViaPdfCommand = new RelayCommand(this.AddInputViaPdf);
            this.Shares = new ObservableCollection<Share>();
            foreach (var share in DataBaseHelper.GetSharesFromDB())
            {
                this.Shares.Add(share);
            }

            this.SelectedShare = this.Shares.First();

            ApplicationService.Instance.EventAggregator.GetEvent<UserChangedEvent>().Subscribe((user) => { this.CurrentUser = user; });
        }

        #endregion

        #region Fields

        /// <summary>
        /// set to true when the <see cref="Dividend"/> was changed to ignore its own refresh in the <see cref="DividendPerShare"/>.
        /// </summary>
        private bool DividendChanged = false;

        /// <summary>
        /// set to true when the <see cref="DividendPerShare"/> was changed to ignore its own refresh in the <see cref="Dividend"/>.
        /// </summary>
        private bool DPSChanged = false;
        #endregion

        #region Properties

        private User _currentUser;

        /// <summary>
        /// Gets or sets the <see cref="User"/> currently selected in the main window.
        /// </summary>
        public User CurrentUser
        {
            get
            {
                if (this._currentUser != null)
                {
                    return this._currentUser;
                }

                return User.Default();
            }

            set
            {
                if (this.CurrentUser != value)
                {
                    this._currentUser = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Share"/>s the user can select to add a dividend for.
        /// </summary>
        public ObservableCollection<Share> Shares { get; set; }

        private double _dividend;

        /// <summary>
        /// Gets or sets the whole dividend for the <see cref="Amount"/> of <see cref="SelectedShare"/>s.
        /// </summary>
        public double Dividend
        {
            get
            {
                return this._dividend;
            }

            set
            {
                if (this._dividend != value)
                {
                    this._dividend = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Dividend)));

                    // dont set "dividend by share" if the dividend has been changed by "dividend per share"
                    if (!this.DPSChanged)
                    {
                        this.DividendChanged = true;
                        this.DividendPerShare = this.Dividend / this.Amount;
                    }

                    this.DPSChanged = false;
                }
            }
        }

        private double _dividendPerShare;

        /// <summary>
        /// Gets or sets the current price of a single share.
        /// </summary>
        public double DividendPerShare
        {
            get
            {
                return this._dividendPerShare;
            }

            set
            {
                if (this._dividendPerShare != value)
                {
                    this._dividendPerShare = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.DividendPerShare)));

                    // refresh the dividend return
                    if (this.Amount != 0 && this.Dividend != 0.0)
                    {
                        this.GetDividendReturnAsync();
                    }

                    // don't set "dividend" if the dividend per share has been changed by "dividend"
                    if (!this.DividendChanged)
                    {
                        if (this.Dividend / this.Amount != this.DividendPerShare)
                        {
                            this.Dividend = this.DividendPerShare * this.Amount;
                            this.DPSChanged = true;
                        }
                    }

                    this.DividendChanged = false;
                }
            }
        }

        private Share _selectedShare;

        /// <summary>
        ///  Gets or sets the <see cref="Share"/> currently selected by the user to add a dividend to.
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
                }
            }
        }

        private double _amount;

        /// <summary>
        /// Gets or sets the amount of shares for which the dividend was given.
        /// </summary>
        public double Amount
        {
            get
            {
                return this._amount;
            }

            set
            {
                if (this._amount != value)
                {
                    this._amount = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Amount)));
                    this.Dividend = this.DividendPerShare * this.Amount;
                }
            }
        }

        private DateTime _dividendPayDate = DateTime.Today;

        /// <summary>
        /// Gets or sets the date at which the dividend was payed.
        /// </summary>
        public DateTime DividendPayDate
        {
            get
            {
                return this._dividendPayDate;
            }

            set
            {
                if (this._dividendPayDate != value)
                {
                    this._dividendPayDate = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.DividendPayDate)));
                }
            }
        }

        private DateTime _dividendRangeStartDate = DateTime.Today.AddYears(-1);

        /// <summary>
        ///  Gets or sets the starting date of the time the share was held.
        /// </summary>
        public DateTime DateRangeStart
        {
            get
            {
                return this._dividendRangeStartDate;
            }

            set
            {
                if (this._dividendRangeStartDate != value)
                {
                    this._dividendRangeStartDate = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.DateRangeStart)));
                }
            }
        }

        private DateTime _dividendRangeEndDate = DateTime.Today;

        /// <summary>
        ///  Gets or sets the last date of the time the share was held.
        /// </summary>
        public DateTime DateRangeEnd
        {
            get
            {
                return this._dividendRangeEndDate;
            }

            set
            {
                if (this._dividendRangeEndDate != value)
                {
                    this._dividendRangeEndDate = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.DateRangeEnd)));
                }
            }
        }

        private double currentPrice;
        private double _dividendReturn;

        /// <summary>
        /// Gets the dividend return (dividen per share/share value).
        /// </summary>
        public double DividendReturn
        {
            get
            {
                return this._dividendReturn;
            }

            private set
            {
                if (this._dividendReturn != value)
                {
                    this._dividendReturn = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.DividendReturn)));
                }
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Sets the <see cref="DividendReturn"/> for the <see cref="SelectedShare"/>.
        /// </summary>
        private async void GetDividendReturnAsync()
        {
            // get the current price of the selected share
            this.currentPrice = await RegexHelper.GetSharePriceAsync(this.SelectedShare);
            if (this.currentPrice == 0.0)
            {
                this.DividendReturn = 0.0;
            }

            // calculate the return (dividenden rendite)
            this.DividendReturn =  this.DividendPerShare / this.currentPrice;
        }

        #endregion

        #region Commands
        public RelayCommand AddDividendCommand { get; private set; }

        public RelayCommand AddInputViaPdfCommand { get; private set; }

        private void AddDividend(object o)
        {
            if (this.CurrentUser.Equals(User.Default()))
            {
                System.Windows.MessageBox.Show("There is no valid user selected");
                return;
            }

            // create a new dividend
            Dividend dividend = new Dividend
            {
                ISIN = this.SelectedShare.ISIN,
                Amount = this.Amount,
                Value = this.Dividend,
                DayOfPayment = this.DividendPayDate,
                DateRangeStart = this.DateRangeStart,
                DateRangeEnd = this.DateRangeEnd,
                UserName = this.CurrentUser.ToString(),
            };

            // add the order to the matching share
            DataBaseHelper.AddDividendToDB(dividend);

            this.Amount = 0;
            this.Dividend = 0.0;
            this.DividendPerShare = 0;
            this.DividendReturn = 0;
        }

        private bool CanAddDividednd(object o)
        {
            return (this.Amount > 0 && this.Dividend > 0.0) ? true : false;
        }

        private void AddInputViaPdf(object o)
        {

            // create the OCR reader
            AdvancedOcr Ocr = new AdvancedOcr()
            {
                CleanBackgroundNoise = false,
                ColorDepth = 8,
                ColorSpace = AdvancedOcr.OcrColorSpace.Color,
                EnhanceContrast = true,
                DetectWhiteTextOnDarkBackgrounds = false,
                RotateAndStraighten = false,
                Language = IronOcr.Languages.German.OcrLanguagePack,
                EnhanceResolution = true,
                InputImageType = AdvancedOcr.InputTypes.Document,
                ReadBarCodes = false,
                Strategy = AdvancedOcr.OcrStrategy.Advanced,
            };

            // create a file dialog
            OpenFileDialog ofd = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                Filter = "PDFs|*.pdf",
                InitialDirectory = @"C:\",
            };

            // when a file was selected....
            if (ofd.ShowDialog() == true)
            {
                var pdfToRead = ofd.FileName;

                try
                {
                    // create a rectangle from which to read (don't set for complete page)
                    System.Drawing.Rectangle area = new System.Drawing.Rectangle(0, 1000, 2400, 1500);
                    var Results = Ocr.ReadPdf(pdfToRead, area, 1);
                    var lines = Results.Pages[0].LinesOfText;

                    // get Amount, ISIN, WKN
                    foreach (var line in lines)
                    {
                        if (line.Text.StartsWith("Stück"))
                        {
                            try
                            {
                                // get ordered amount
                                var strAmount = line.Words[1].Text;
                                int intAmount = 0;
                                int.TryParse(strAmount, out intAmount);
                                this.Amount = intAmount;

                                // Share by ISIN or WKN
                                var isin = line.Words[line.WordCount - 2].Text;
                                var wkn = line.Words.Last().Text.Replace("(", string.Empty).Replace(")", string.Empty);

                                // try to match a Share already in the database
                                // first by ISIN
                                var sharesByIsin = DataBaseHelper.GetSharesFromDB().Where((s) => { return s.ISIN == isin; });
                                if (sharesByIsin.Count() != 0)
                                {
                                    this.SelectedShare = sharesByIsin.First();
                                    break;
                                }

                                // if none is found by ISIN try by WKN
                                var sharesByWkn = DataBaseHelper.GetSharesFromDB().Where((s) => { return s.WKN == wkn; });
                                if (sharesByWkn.Count() != 0)
                                {
                                    this.SelectedShare = sharesByWkn.First();
                                    break;
                                }

                                // if none is found by WKN try by ISIN with "O" replaced by zeros
                                var sharesByIsin0 = DataBaseHelper.GetSharesFromDB().Where((s) => { return s.ISIN == isin.Replace("O", "0"); });
                                if (sharesByIsin0.Count() != 0)
                                {
                                    this.SelectedShare = sharesByIsin0.First();
                                    break;
                                }

                                // if none is found try by WKN with "O" replaced by zeros
                                var sharesByWkn0 = DataBaseHelper.GetSharesFromDB().Where((s) => { return s.WKN == wkn.Replace("O", "0"); });
                                if (sharesByWkn0.Count() != 0)
                                {
                                    this.SelectedShare = sharesByWkn0.First();
                                    break;
                                }
                            }
                            catch (Exception)
                            {
                                MessageBox.Show("Error on reading amount, ISIN or WKN");
                            }

                            // if none is found, don't select any order
                            break;
                        }
                    }

                    // get dividend
                    foreach (var line in lines)
                    {
                        if (line.Text.StartsWith("Ausmachender Betrag"))
                        {
                            try
                            {
                                // get share price
                                var strPrice = line.Words[2].Text.Replace("+", string.Empty);
                                double doublePrice = 0.0;
                                double.TryParse(strPrice, out doublePrice);
                                this.DividendChanged = false;
                                this.DPSChanged = false;
                                this.Dividend = doublePrice;
                            }
                            catch (Exception)
                            {
                                MessageBox.Show("Error on reading of whole dividend");
                            }

                            break;
                        }
                    }

                    // get dividend pay date
                    foreach (var line in lines)
                    {
                        if (line.Text.StartsWith("Bestandsstichtag"))
                        {
                            try
                            {
                                var match = Regex.Match(line.Text, "\\d{2}\\.\\d{2}.\\d{4}");
                                // get share price
                                var strDate = match.Value;
                                DateTime Date;
                                DateTime.TryParse(strDate, out Date);
                                this.DividendPayDate = Date;
                            }
                            catch (Exception)
                            {
                                MessageBox.Show("Error on reading of booking date");
                            }

                            break;
                        }
                    }

                    // get dividend Start Range Date and End Range Date
                    foreach (var line in lines)
                    {
                        if (line.Text.StartsWith("Geschäftsjahr"))
                        {
                            try
                            {
                                var matches = Regex.Matches(line.Text, "\\d{2}\\.\\d{2}.\\d{4}");

                                // get share price
                                var strDate = matches[0].Value;
                                DateTime Date;
                                DateTime.TryParse(strDate, out Date);
                                this.DateRangeStart = Date;

                                strDate = matches[1].Value;
                                DateTime.TryParse(strDate, out Date);
                                this.DateRangeEnd = Date;
                            }
                            catch (Exception)
                            {
                                MessageBox.Show("Error on reading dividend timespan");
                            }

                            break;
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Error on reading pdf");
                }

                // time for OCR ~8s ... animation für busy einbauen?

                // < Style >
                //    < Style.Triggers >
                //        < DataTrigger Binding = "{Binding IsAnimationRunning}" Value = "True" >
                //               < DataTrigger.EnterActions >
                //                   < BeginStoryboard >
                //                       < Storyboard >
                //                           < SomeAnimation />
                //                       </ Storyboard >
                //                   </ BeginStoryboard >
                //               </ DataTrigger.EnterActions >
                //           </ DataTrigger >
                //       </ Style.Triggers >
                //   </ Style >
            }
        }
        #endregion
    }
}
