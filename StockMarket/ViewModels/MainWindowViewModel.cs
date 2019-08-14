using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Linq;
using IronOcr;

namespace StockMarket.ViewModels
{
    /// <summary>
    /// a view model for the main window
    /// </summary>
    class MainWindowViewModel: ViewModels.ViewModelBase
    {          
        /// <summary>
        /// creates a new instance of a ViewModel for the MainWindow
        /// </summary>
        public MainWindowViewModel()
        {

            // TODO: ShareOverview add overview tiles sold, shares, certificates
            // TODO: single share Graph


            // set the start page to an empty page
            DisplayPage = new Pages.BlankPage();

            // define the commands for the buttons
            AddShareCommand = new RelayCommand(AddShare);
            AddOrderCommand = new RelayCommand(AddOrder,CanAddOrder);
            DisplayOrderOverviewCommand = new RelayCommand(DisplayOrderOverview, CanDisplayOrderOverview);
            DisplayShareOverviewCommand = new RelayCommand(DisplayShareOverview, CanDisplayShareOverview);
            DisplayShareDetailCommand = new RelayCommand(DisplayShareDetail, CanDisplayShareDetail);

            // create a timer for updating the Sharevalues in the database
            DispatcherTimer t = new DispatcherTimer();
            t.Interval = new System.TimeSpan(0, 20, 0);
            t.Tick += TimerTick;
            t.Start();

            // try to update share values once at program start
            TimerTick(null, null);

            //TODO: OCR on PDF
            //var Ocr = new AdvancedOcr()
            //{
            //    CleanBackgroundNoise = false,
            //    ColorDepth = 4,
            //    ColorSpace = AdvancedOcr.OcrColorSpace.Color,
            //    EnhanceContrast = false,
            //    DetectWhiteTextOnDarkBackgrounds = false,
            //    RotateAndStraighten = false,
            //    Language = IronOcr.Languages.German.OcrLanguagePack,
            //    EnhanceResolution = false,
            //    InputImageType = AdvancedOcr.InputTypes.Document,
            //    ReadBarCodes = false,
            //    Strategy = AdvancedOcr.OcrStrategy.Fast
            //};

            //System.Drawing.Rectangle area = new System.Drawing.Rectangle(0, 0, 2400, 800);
            //var testImage = @"C:\Users\vm_user\Downloads\datasheet.pdf";
            //var Results = Ocr.ReadPdf(testImage,area,1);
            //var Pages = Results.Pages;
            //var FullPdfText = Results.Text;

            //Console.WriteLine(Results.Text);

        }

        #region events
        /// <summary>
        /// Eventhandler for updating the Sharevalues in the database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TimerTick(object sender, System.EventArgs e)
        {                  
            // if its a friday night...
            if (/*DateTime.Now.DayOfWeek == DayOfWeek.Friday*/ 
                DateTime.Now.DayOfWeek != DayOfWeek.Saturday &&
                DateTime.Now.DayOfWeek != DayOfWeek.Sunday &&
                DateTime.Now.Hour>=22)
            {
                //... get all shares in the portfolio
                var shares = DataBaseHelper.GetSharesFromDB();

                // for each of these shares...
                foreach (var share in shares)
                {
                    // ...get the latest value in the database
                    var latestValues = DataBaseHelper.GetShareValuesFromDB(share)?.OrderByDescending((v) => v.Date);
                    if (latestValues.Count() > 0)
                    {
                        var latestValue = latestValues.First();
                        // if it is from today...
                        if (latestValue?.Date.Date == DateTime.Today)
                        {   //... we can ignore the following and continue with the next share
                            continue;
                        }
                    }
                
                    //... if it is not from today get the current price of the share
                    var webcontent = await WebHelper.getWebContent(share.WebSite);
                    var price = RegexHelper.GetSharePrice(webcontent, share.ShareType);

                    // create a new sharevalue
                    ShareValue s = new ShareValue()
                    {
                        Date = DateTime.Today,
                        ISIN = share.ISIN,
                        Price = price                    
                    };

                    // and add it to the database
                    DataBaseHelper.AddShareValueToDB(s);                  

                }
            }
        }
        #endregion

        #region Properties
        private Page _displayPage;
        /// <summary>
        /// The <see cref="Page"/> to display in the main frame
        /// </summary>
        public Page DisplayPage
        {
            get
            {
                return _displayPage;
            }
            private set
            {
                if (_displayPage != value)
                {
                    _displayPage = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(DisplayPage)));
                }
            }
        }
        #endregion

        #region Commands             

        #region Add Share Command        
        public RelayCommand AddShareCommand { get; private set; }

        private void AddShare(object o)
        {
            DisplayPage = new Pages.AddSharePage();
        }
        #endregion

        #region Add Order Command
        public RelayCommand AddOrderCommand { get; private set; }

        private void AddOrder(object o)
        {
            DisplayPage = new Pages.AddOrderPage();
        }

        private bool CanAddOrder(object o)
        {
            int shares = 0;
            // try-catch only for UI generator
            try
            {
                shares = DataBaseHelper.GetSharesFromDB().Count;
            }
            catch
            {
                return true;
            }
            // execute only when we have at least one share
            return shares > 0;
        }
        #endregion

        #region Display Share Detail Command
        public RelayCommand DisplayShareDetailCommand { get; private set; }

        private void DisplayShareDetail(object o)
        {
            DisplayPage = new Pages.ShareDetailPage();
        }

        private bool CanDisplayShareDetail(object o)
        {
            int count = 0;
            // try-catch only for UI generator
            try
            {
                count = DataBaseHelper.GetSharesFromDB().Count;
            }
            catch
            {
                return true;
            }
            // execute only when we have at least one share
            return count > 0;
        }
        #endregion


        #region Display Order Gain Command
        public RelayCommand DisplayOrderOverviewCommand { get; private set; }

        private void DisplayOrderOverview(object o)
        {
            DisplayPage = new Pages.OrdersOverviewPage();
        }

        private bool CanDisplayOrderOverview(object o)
        {
            int orders = 0;

            // try-catch only for UI generator
            try
            {
                var shares = DataBaseHelper.GetSharesFromDB();
                foreach (var share in shares)
                {
                    orders += DataBaseHelper.GetOrdersFromDB(share).Count;
                }
            }
            catch
            {
                return true;
            }

            // execute only when we have at least one order of any share
            return orders > 0;
        }
        #endregion

        #region Display Share Gain Command
        public RelayCommand DisplayShareOverviewCommand { get; private set; }

        private void DisplayShareOverview(object o)
        {
            DisplayPage = new Pages.SharesOverviewPage();
        }

        private bool CanDisplayShareOverview(object o)
        {
            int count = 0;
            // try-catch only for UI generator
            try
            {
                count = DataBaseHelper.GetSharesFromDB().Count;
            }
            catch
            {
                return true;
            }
            // execute only when we have at least one share
            return count > 0;
        } 
        #endregion

        #endregion
    }
}
