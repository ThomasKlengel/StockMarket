using SQLite;

namespace StockMarket
{
    /// <summary>
    /// A class containing basic information about a share.
    /// </summary>
    public class Share : IHasIsin
    {

        #region ctors

        /// <summary>
        /// Initializes a new instance of the <see cref="Share"/> class.
        /// </summary>
        public Share()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Share"/> class.
        /// </summary>
        /// <param name="name">The name of the share as displayed to the user.</param>
        /// <param name="website">The website where to look for a current price.</param>
        /// <param name="wkn">The WKN of the share.</param>
        /// <param name="isin">The ISIN of the share.</param>
        /// <param name="shareType">The sharetype.</param>
        /// <param name="website2">A non mandatory secondary website.</param>
        /// <param name="website3">A non mandatory third website.</param>
        public Share(string name, string website, string wkn, string isin, ShareType shareType = ShareType.Share, string website2 = "", string website3 = "")
        {
            this.ShareName = name;
            this.WebSite = website;
            this.WebSite2 = website2;
            this.WebSite3 = website3;
            this.WKN = wkn;
            this.ISIN = isin;
            this.ShareType = shareType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of the stock company.
        /// </summary>
        public string ShareName { get; set; }

        /// <summary>
        /// Gets or sets a website from which to get the data for the <see cref="Share"/>.
        /// </summary>
        public string WebSite { get; set; }

        /// <summary>
        /// Gets or sets a website from which to get the data for the <see cref="Share"/>.
        /// </summary>
        public string WebSite2 { get; set; }

        /// <summary>
        /// Gets or sets a website from which to get the data for the <see cref="Share"/>.
        /// </summary>
        public string WebSite3 { get; set; }

        /// <summary>
        /// Gets or sets thw WKN of the <see cref="Share"/>.
        /// </summary>
        public string WKN { get; set; }

        /// <summary>
        /// Gets or sets the ISIN of the <see cref="Share"/>.
        /// </summary>
        [PrimaryKey]
        public string ISIN { get; set; }

        /// <summary>
        /// Gets or sets the type of the share (Share, Certificate).
        /// </summary>
        public ShareType ShareType { get; set; }
        #endregion

        #region Methods

        /// <summary>
        /// Checks the equality of two shares by their ISIN.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) =>
            obj is Share s
            ? this.ISIN == s.ISIN
            : false;


        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

    }

    /// <summary>
    /// The type of the share
    /// Share=0,
    /// Certificate=1,
    /// ETF = 2.
    /// </summary>
    public enum ShareType
    {
        /// <summary>
        /// A normal share
        /// </summary>
        Share = 0,

        /// <summary>
        /// A certificate for a share
        /// </summary>
        Certificate = 1,

        /// <summary>
        /// A exchange traded fund
        /// </summary>
        ETF = 2,
    }
}
