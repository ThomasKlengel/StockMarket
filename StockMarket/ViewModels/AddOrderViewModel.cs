﻿using IronOcr;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// A ViewModel for a single <see cref="Order"/>
    /// </summary>
    public class AddOrderViewModel : ViewModelBase
    {
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
        }

        #endregion

        #region Properties

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
        /// The current price of a single share
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
        public Share SelectedShare
        {
            get { return _selectedShare; }
            set
            {
                if (_selectedShare != value)
                {
                    _selectedShare = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedShare)));
                    RefreshPriceAsync();
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
                }
            }
        }             

        private OrderType _orderType = OrderType.buy;
        /// <summary>
        /// The type of order
        /// </summary>
        public OrderType OrderType
        {
            get { return _orderType; }
            set
            {
                if (_orderType != value)
                {
                    _orderType = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(OrderType)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(OrderIsBuy)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(OrderType)));
                }
            }
        }


        public bool OrderIsBuy
        {
            get
            {
                return OrderType == OrderType.buy;
            }
            set
            {
                if (value) // ignore the when bound CheckBox is unchecked
                {
                    OrderType = OrderType == OrderType.sell ? OrderType.buy : OrderType.sell;
                }
            }
        }
        public bool OrderIsSell
        {
            get
            {
                return OrderType == OrderType.sell;
            }
            set
            {
                if (value) // ignore the when bound CheckBox is unchecked
                {
                    OrderType = OrderType == OrderType.sell ? OrderType.buy : OrderType.sell;
                }
            }
        }

        private DateTime _dateTime = DateTime.Today;
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
        private async void RefreshPriceAsync()
        {
            if (SelectedShare != null)
            {
                double price = 0.0;
                var content = await WebHelper.getWebContent(SelectedShare.WebSite);
                price = RegexHelper.GetSharePrice(content, SelectedShare.ShareType);
                ActPrice = price;
            }
        }
        #endregion

        #region Commands
        public RelayCommand AddOrderCommand { get; private set; }

        public RelayCommand AddInputViaPdfCommand { get; private set; }

        private void AddOrder(object o)
        {
            // create a new order
            Order order = new Order();
            order.Amount = Amount;
            order.OrderExpenses = Expenses;
            order.OrderType = OrderType;
            order.SharePrice = ActPrice;
            order.Date = OrderDate;
            order.ISIN = SelectedShare.ISIN;

            // add the order to the matching share
            DataBaseHelper.AddOrderToDB(order);

            Amount = 0;
        }

        private bool CanAddOrder(object o)
        {
            return Amount > 0 ? true : false;
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

                //get order type
                foreach (var line in lines)
                {
                    if (line.Text.StartsWith("Wertpapier Abrechnung"))
                    {
                        var buySell = lines.First().Words.Last().Text;
                        OrderIsBuy = buySell == "Verkauf" ? false : true;
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
                        Int32.TryParse(strAmount, out intAmount);
                        Amount = intAmount;

                        // Share by ISIN or WKN
                        var isin = line.Words[(line.WordCount - 2)].Text;
                        var wkn = line.Words.Last().Text.Replace("(", "").Replace(")", "");

                        var sharesByIsin = (DataBaseHelper.GetSharesFromDB().Where((s) => { return s.ISIN == isin; }));  
                        if (sharesByIsin.Count() != 0)
                        {
                            _selectedShare = sharesByIsin.First();
                            break;
                        }
                        var sharesByWkn = (DataBaseHelper.GetSharesFromDB().Where((s) => { return s.WKN == wkn; }));
                        if (sharesByWkn.Count() != 0)
                        {
                            _selectedShare = sharesByWkn.First();
                            break;
                        }
                        var sharesByIsin0 = (DataBaseHelper.GetSharesFromDB().Where((s) => { return s.ISIN == isin.Replace("O", "0"); }));
                        if (sharesByIsin0.Count() != 0)
                        {
                            _selectedShare = sharesByIsin0.First();
                            break;
                        }
                        var sharesByWkn0 = (DataBaseHelper.GetSharesFromDB().Where((s) => { return s.WKN == wkn.Replace("O", "0"); }));
                        if (sharesByWkn0.Count() != 0)
                        {
                            _selectedShare = sharesByWkn0.First();
                            break;
                        }
                        break;
                    }
                }

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
                }

                // get expenses
                int lineIndex = 0;
                foreach (var line in lines)
                {
                    int provisionLineIndex = 1000;
                    if (line.Text.StartsWith("Provision"))
                    {
                        provisionLineIndex = lineIndex;
                        // get order expenses
                        var strExpense = line.Words[1].Text;
                        double doubleExpense = 0.0;
                        Double.TryParse(strExpense, out doubleExpense);
                        Expenses = doubleExpense;
                    }

                    if (provisionLineIndex<100)
                    {
                        if (line.Text.StartsWith("Ausmachender") || line.Text.StartsWith("Ermittlung"))
                        {
                            break;
                        }

                        if (line.Words[line.Words.Count()-2].Text=="-")
                        {
                            // get additional expenses
                            var strExpense = line.Words[line.Words.Count() - 3].Text;
                            double doubleExpense = 0.0;
                            Double.TryParse(strExpense, out doubleExpense);
                            Expenses += doubleExpense;
                        }

                    }
                    lineIndex++;
                }



                //Wertpapier Abrechnung Kauf
                //Nominale Wertpapierbezeichnung ISIN(WKN)                
                //Stück 80 UBS AG(LONDON BRANCH) DEOOOUFOAA67(UFOAA6)  --> replace  "O" durch "0" wkn,isin match share by isin -> nomatch: wkn
                //FAKTL O.END AMAZON                
                //Handels -/ Ausführungsplatz Frankfurt(gemäß Weisung)
                //Börsensegment FRAB                
                //Market - Order                
                //Limit billigst
                //Schlusstagl - Zeit 23.05.201919:46:13 Auftraggeber Vorname Nachname
                //Ausführungskurs 6,15 EUR Auftragserteilung/ -ort Online - Banking                
                //Girosammelverw.mehrere Sammelurkunden -kein Stückeausdruck —                
                //Kurswert 492,00 - EUR
                //Provision 10,00 - EUR
                //Ausmachender Betrag 502,00 - EUR                
                //Den Gegenwert buchen wir mit Valuta 27.05.2019 zu Lasten des Kontos xxxxxxxx04
                //(IBAN DE77 xxxx xxxx xxxx xxxx 04), BLZ xxxxxxxx(BIC xxxxxxxxx).
                //Die Wertpapiere schreiben wir Ihrem Depotkonto gut.

                //Wertpapier Abrechnung Verkauf
                //Nominale Wertpapierbezeichnung ISIN (WKN)
                //Stück 10 UBISOFT ENTERTAINMENT S.A. FR0000054470 (901581)
                //ACTIONS PORT. EO 0,0775                
                //Handels -/ Ausführungsplatz Frankfurt(gemäß Weisung)
                //Börsensegment FRAB                
                //Market - Order                
                //Limit bestens
                //Schlusstagl - Zeit 26.04.2019 12:46:53 Auftraggeber Vorname Nachname
                //Ausführungskurs 83,70 EUR Auftragserteilung/ -ort Online—Banking                
                //Girosammelverw.mehrere Sammelurkunden -kein Stückeausdruck -                
                //Kurswert 837,00 EUR
                //Provision 10,00 - EUR
                //Transaktionsentgeltßörse 0,71 - EUR
                //Ubertragungs -/ Liefergebühr 0,13 - EUR
                //Handelsentgelt 3,00 - EUR
                //Ermittlung steuerrelevante Erträge                
                //Veräußerungsverlust 164,99 - EUR                
                //Eingebuchte Aktienverluste 164,99 EUR                
                //Ausmachender Betraa 823.16 EUR


                var completeText = Results.Pages[0].Text;

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
