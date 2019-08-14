using IronOcr;
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

            //TODO: OCR on PDF

            //create the OCR reader
            AdvancedOcr Ocr = new AdvancedOcr()
            {
                CleanBackgroundNoise = false,
                ColorDepth = 4,
                ColorSpace = AdvancedOcr.OcrColorSpace.Color,
                EnhanceContrast = false,
                DetectWhiteTextOnDarkBackgrounds = false,
                RotateAndStraighten = false,
                Language = IronOcr.Languages.German.OcrLanguagePack,
                EnhanceResolution = false,
                InputImageType = AdvancedOcr.InputTypes.Document,
                ReadBarCodes = false,
                Strategy = AdvancedOcr.OcrStrategy.Fast
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
                System.Drawing.Rectangle area = new System.Drawing.Rectangle(0, 0, 2400, 800);
                var t1 = DateTime.Now;
                var Results = Ocr.ReadPdf(pdfToRead, area, 1);                
                var Words = Results.Pages[0].Words;
                var t2 = DateTime.Now;
                var lines = Results.Pages[0].LinesOfText;
                var completeText = Results.Pages[0].Text;

                var dt = t2 - t1; // ~8s ... animation für busy einbauen?

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
