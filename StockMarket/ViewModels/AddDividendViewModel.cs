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
    public class AddDividendViewModel : ViewModelBase
    {

        #region ctor
        public AddDividendViewModel()
        {
            AddDividendCommand = new RelayCommand(AddDividend, CanAddDividednd);
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

        public ObservableCollection<Share> Shares { get; set; }

        private double _dividend;
        /// <summary>
        /// The current price of a single share
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
                    if (Dividend / Amount != DividendPerShare)
                    {
                        DividendPerShare = Dividend / Amount;
                    }
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
                if (_dividendPerShare!= value)
                {
                    _dividendPerShare = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(DividendPerShare)));
                    if (Dividend / Amount != DividendPerShare)
                    {
                        Dividend = DividendPerShare * Amount;
                    }
                }
            }
        }

        private Share _selectedShare;
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

        private int _amount;
        /// <summary>
        /// The amount of shares purchased
        /// </summary>
        public int Amount
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
                Amount = Amount,
                Value = Dividend,
                DayOfPayment = DividendPayDate,
                ISIN = SelectedShare.ISIN,
                DateRangeStart = DateRangeStart,
                DateRangeEnd = DateRangeEnd,
                UserName = CurrentUser.ToString()
            };

            // add the order to the matching share
            DataBaseHelper.AddDividendToDB(dividend);

            Amount = 0;
            Dividend = 0.0;
        }

        private bool CanAddDividednd(object o)
        {
            return (Amount > 0 && Dividend > 0.0) ? true : false;
        }

        private void AddInputViaPdf(object o)
        {

            //create the OCR reader
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
                System.Drawing.Rectangle area = new System.Drawing.Rectangle(0, 1000, 2400, 1500);                
                var Results = Ocr.ReadPdf(pdfToRead, area, 1);                
                var lines = Results.Pages[0].LinesOfText;
                
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
                }

                //get dividend
                foreach (var line in lines)
                {
                    if (line.Text.StartsWith("Ausführungskurs"))
                    {
                        // get share price
                        var strPrice = line.Words[1].Text;
                        double doublePrice = 0.0;
                        Double.TryParse(strPrice, out doublePrice);
                        Dividend = doublePrice;
                        break;
                    }
                }

                //get dividend pay date
                foreach (var line in lines)
                {
                    if (line.Text.StartsWith("Schluss"))
                    {

                        var match = Regex.Match(line.Text, "\\d{2}\\.\\d{2}.\\d{4}");
                        // get share price
                        var strDate = match.Value;
                        DateTime Date;
                        DateTime.TryParse(strDate, out Date );                        
                        DividendPayDate = Date;
                        break;
                    }
                }




                // time for OCR ~8s ... animation für busy einbauen?

                //< Style >
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

                //TODO: search relevant parts of text 


            }
        }
        #endregion
    }
}
