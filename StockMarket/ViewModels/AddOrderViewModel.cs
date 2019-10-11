using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using IronOcr;
using Microsoft.Win32;
using Prism.Events;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for a single <see cref="Order"/>.
    /// </summary>
    public class AddOrderViewModel : ViewModelBase
    {
        /// <summary>
        /// set to true when a <see cref="Share"/> is selected from reading a pdf
        /// to irgnore auto updates for <see cref="ActPrice"/>, etc.
        /// </summary>
        bool ignoreUpdate = false;

        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AddOrderViewModel"/> class.
        /// </summary>
        public AddOrderViewModel()
        {
            this.AddOrderCommand = new RelayCommand(this.AddOrder, this.CanAddOrder);
            this.AddInputViaPdfCommand = new RelayCommand(this.AddInputViaPdf);
            this.Shares = new ObservableCollection<Share>();
            foreach (var share in  DataBaseHelper.GetSharesFromDB() )
            {
                this.Shares.Add(share);
            }

            this.SelectedShare = this.Shares.First();

            ApplicationService.Instance.EventAggregator.GetEvent<UserChangedEvent>().Subscribe((user) => { this.CurrentUser = user; });
        }

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
        /// Gets or sets the <see cref="Share"/>s the user can choose from to add an orders to.
        /// </summary>
        public ObservableCollection<Share> Shares { get; set; }

        private double _actPrice;

        /// <summary>
        /// Gets or sets the current price of a single share.
        /// </summary>
        public double ActPrice
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
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.ActPrice)));
                }
            }
        }

        private double _expenses = 10.0;

        /// <summary>
        /// Gets or sets the expenses for any transaction.
        /// </summary>
        public double Expenses
        {
            get
            {
                return this._expenses;
            }

            set
            {
                if (this._expenses != value)
                {
                    this._expenses = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Expenses)));
                }
            }
        }

        private Share _selectedShare;

        /// <summary>
        /// Gets or sets the <see cref="Share"/> that is currently selected in the UI to add the orders to.
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
                    // update when Share is selected by user, not when reading from PDF
                    if (!this.ignoreUpdate)
                    {
                        this.GetPriceAsync();
                        this.Expenses = 10;
                        this.Amount = 0;
                        this.OrderDate = DateTime.Today;
                        this.OrderType = ShareComponentType.Buy;
                    }

                    this.ignoreUpdate = false;
                }
            }
        }

        private double _amount;

        /// <summary>
        /// Gets or sets the amount of shares purchased.
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
                }
            }
        }

        private ShareComponentType _orderType = ShareComponentType.Buy;

        /// <summary>
        /// Gets or sets the type of order.
        /// </summary>
        public ShareComponentType OrderType
        {
            get
            {
                return this._orderType;
            }

            set
            {
                if (this._orderType != value)
                {
                    this._orderType = value;
                    // Update the Checkboxes
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.OrderType)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.OrderIsBuy)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.OrderIsSell)));
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether used to check the buy/sell-checkbox.
        /// </summary>
        public bool OrderIsBuy
        {
            get
            {
                return this.OrderType == ShareComponentType.Buy;
            }

            set
            {
                if (value) // dont update when bound CheckBox is unchecked
                {
                    this.OrderType = this.OrderType == ShareComponentType.Sell ? ShareComponentType.Buy : ShareComponentType.Sell;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether used to check the buy/sell-checkbox.
        /// </summary>
        public bool OrderIsSell
        {
            get
            {
                return this.OrderType == ShareComponentType.Sell;
            }

            set
            {
                if (value) // dont update when bound CheckBox is unchecked
                {
                    this.OrderType = this.OrderType == ShareComponentType.Sell ? ShareComponentType.Buy : ShareComponentType.Sell;
                }
            }
        }

        private DateTime _dateTime = DateTime.Today;

        /// <summary>
        /// Gets or sets the date at which the order has been booked.
        /// </summary>
        public DateTime OrderDate
        {
            get
            {
                return this._dateTime;
            }

            set
            {
                if (this._dateTime != value)
                {
                    this._dateTime = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.OrderDate)));
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the current price of the <see cref="SelectedShare"/>.
        /// </summary>
        private async void GetPriceAsync()
        {
            this.ActPrice = await RegexHelper.GetSharePriceAsync(this.SelectedShare);
        }

        #endregion

        #region Commands
        public RelayCommand AddOrderCommand { get; private set; }

        public RelayCommand AddInputViaPdfCommand { get; private set; }

        private void AddOrder(object o)
        {
            // cehck if a valid user is selected
            if (this.CurrentUser.Equals(User.Default()))
            {
                System.Windows.MessageBox.Show("There is no valid user selected");
                return;
            }

            // create a new order
            Order order = new Order();
            order.Amount = this.Amount;
            order.OrderExpenses = this.Expenses;
            order.OrderType = this.OrderType;
            order.SharePrice = this.ActPrice;
            order.Date = this.OrderDate;
            order.ISIN = this.SelectedShare.ISIN;
            order.UserName = this.CurrentUser.ToString();

            // add the order to the matching share
            DataBaseHelper.AddOrderToDB(order);

            // set Amount to zero so that it cant be added by accident twice
            this.Amount = 0;
        }

        private bool CanAddOrder(object o)
        {
            // anly add if the amount of ordered Shares is more then zero
            return this.Amount > 0 ? true : false;
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

            if (ofd.ShowDialog() == true)
            {
                var pdfToRead = ofd.FileName;

                // create a rectangle from which to read (dont set for complete page)
                System.Drawing.Rectangle area = new System.Drawing.Rectangle(0, 1000, 2400, 1500);
                var Results = Ocr.ReadPdf(pdfToRead, area, 1);
                var lines = Results.Pages[0].LinesOfText;

                // get order type
                foreach (var line in lines)
                {
                    if (line.Text.StartsWith("Wertpapier Abrechnung"))
                    {
                        var buySell = line.Words.Last().Text;
                        this.OrderType = buySell == "Verkauf" ? ShareComponentType.Sell : ShareComponentType.Buy;
                        break;
                    }
                }

                // get Amount, ISIN, WKN
                foreach (var line in lines)
                {
                    if (line.Text.StartsWith("Stück"))
                    {
                        // get ordered amount
                        var strAmount = line.Words[1].Text;
                        int intAmount = 0;
                        int.TryParse(strAmount, out intAmount);
                        this.Amount = intAmount;

                        // Share by ISIN or WKN
                        var isin = line.Words[line.WordCount - 2].Text;
                        var wkn = line.Words.Last().Text.Replace("(", string.Empty).Replace(")", string.Empty);

                        this.ignoreUpdate = true;
                        var sharesByIsin = DataBaseHelper.GetSharesFromDB().Where((s) => { return s.ISIN == isin; });
                        if (sharesByIsin.Count() != 0)
                        {
                            this.SelectedShare = sharesByIsin.First();
                            break;
                        }

                        var sharesByWkn = DataBaseHelper.GetSharesFromDB().Where((s) => { return s.WKN == wkn; });
                        if (sharesByWkn.Count() != 0)
                        {
                            this.SelectedShare = sharesByWkn.First();
                            break;
                        }

                        var sharesByIsin0 = DataBaseHelper.GetSharesFromDB().Where((s) => { return s.ISIN == isin.Replace("O", "0"); });
                        if (sharesByIsin0.Count() != 0)
                        {
                            this.SelectedShare = sharesByIsin0.First();
                            break;
                        }

                        var sharesByWkn0 = DataBaseHelper.GetSharesFromDB().Where((s) => { return s.WKN == wkn.Replace("O", "0"); });
                        if (sharesByWkn0.Count() != 0)
                        {
                            this.SelectedShare = sharesByWkn0.First();
                            break;
                        }

                        break;
                    }
                }

                // get SharePrice at ordertime
                foreach (var line in lines)
                {
                    if (line.Text.StartsWith("Ausführungskurs"))
                    {
                        // get share price
                        var strPrice = line.Words[1].Text;
                        double doublePrice = 0.0;
                        double.TryParse(strPrice, out doublePrice);
                        this.ActPrice = doublePrice;
                        break;
                    }
                }

                // get orderdate
                foreach (var line in lines)
                {
                    if (line.Text.StartsWith("Schluss"))
                    {

                        var match = Regex.Match(line.Text, "\\d{2}\\.\\d{2}.\\d{4}");
                        // get share price
                        var strDate = match.Value;
                        DateTime Date;
                        DateTime.TryParse(strDate, out Date );
                        this.OrderDate = Date;
                        break;
                    }
                }

                // get expenses
                int lineIndex = 0;
                int provisionLineIndex = 1000;
                foreach (var line in lines)                {

                    if (line.Text.StartsWith("Provision"))
                    {
                        provisionLineIndex = lineIndex;
                        // get order expenses
                        var strExpense = line.Words[1].Text.Replace("-",string.Empty);
                        double doubleExpense = 0.0;
                        double.TryParse(strExpense, out doubleExpense);
                        this.Expenses = doubleExpense;
                        lineIndex++;
                        continue;
                    }

                    if (provisionLineIndex < 100)
                    {
                        if (line.Text.StartsWith("Ausmachender") || line.Text.StartsWith("Ermittlung"))
                        {
                            break;
                        }

                        if (line.Words[line.Words.Count() - 2].Text.EndsWith("-"))
                        {
                            // get additional expenses
                            var strExpense = line.Words[line.Words.Count() - 2].Text.Replace("-",string.Empty);
                            double doubleExpense = 0.0;
                            double.TryParse(strExpense, out doubleExpense);
                            this.Expenses += doubleExpense;
                        }
                    }

                    lineIndex++;
                }

                // Wertpapier Abrechnung Kauf
                // Nominale Wertpapierbezeichnung ISIN(WKN)
                // Stück 80 UBS AG(LONDON BRANCH) DEOOOUFOAA67(UFOAA6)  --> replace  "O" durch "0" wkn,isin match share by isin -> nomatch: wkn
                // FAKTL O.END AMAZON
                // Handels -/ Ausführungsplatz Frankfurt(gemäß Weisung)
                // Börsensegment FRAB
                // Market - Order
                // Limit billigst
                // Schlusstagl - Zeit 23.05.201919:46:13 Auftraggeber Vorname Nachname
                // Ausführungskurs 6,15 EUR Auftragserteilung/ -ort Online - Banking
                // Girosammelverw.mehrere Sammelurkunden -kein Stückeausdruck —
                // Kurswert 492,00 - EUR
                // Provision 10,00 - EUR
                // Ausmachender Betrag 502,00 - EUR
                // Den Gegenwert buchen wir mit Valuta 27.05.2019 zu Lasten des Kontos xxxxxxxx04
                // (IBAN DE77 xxxx xxxx xxxx xxxx 04), BLZ xxxxxxxx(BIC xxxxxxxxx).
                // Die Wertpapiere schreiben wir Ihrem Depotkonto gut.

                // Wertpapier Abrechnung Verkauf
                // Nominale Wertpapierbezeichnung ISIN (WKN)
                // Stück 10 UBISOFT ENTERTAINMENT S.A. FR0000054470 (901581)
                // ACTIONS PORT. EO 0,0775
                // Handels -/ Ausführungsplatz Frankfurt(gemäß Weisung)
                // Börsensegment FRAB
                // Market - Order
                // Limit bestens
                // Schlusstagl - Zeit 26.04.2019 12:46:53 Auftraggeber Vorname Nachname
                // Ausführungskurs 83,70 EUR Auftragserteilung/ -ort Online—Banking
                // Girosammelverw.mehrere Sammelurkunden -kein Stückeausdruck -
                // Kurswert 837,00 EUR
                // Provision 10,00 - EUR
                // Transaktionsentgeltßörse 0,71 - EUR
                // Ubertragungs -/ Liefergebühr 0,13 - EUR
                // Handelsentgelt 3,00 - EUR
                // Ermittlung steuerrelevante Erträge
                // Veräußerungsverlust 164,99 - EUR
                // Eingebuchte Aktienverluste 164,99 EUR
                // Ausmachender Betraa 823.16 EUR
                var completeText = Results.Pages[0].Text;

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
