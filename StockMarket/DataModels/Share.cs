using SQLite;

namespace StockMarket
{
    /// <summary>
    /// A class containing basic information about a share
    /// </summary>
    public class Share: IHasIsin
    {

        #region ctors
        public Share() { }

        public Share(string name, string website, string wkn, string isin, ShareType shareType= ShareType.Share, string website2 = "", string website3 = "")
        {
            ShareName = name;
            WebSite = website;
            WebSite2 = website2;
            WebSite3 = website3;
            WKN = wkn;
            ISIN = isin;
            ShareType = shareType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The name of the stock company
        /// </summary>
        public string ShareName { get; set; }

        /// <summary>
        /// A website from which to get the data for the <see cref="Share"/>
        /// </summary>
        public string WebSite { get; set; }

        /// <summary>
        /// A website from which to get the data for the <see cref="Share"/>
        /// </summary>
        public string WebSite2 { get; set; }

        /// <summary>
        /// A website from which to get the data for the <see cref="Share"/>
        /// </summary>
        public string WebSite3 { get; set; }

        /// <summary>
        /// Thw WKN of the <see cref="Share"/>
        /// </summary>
        public string WKN { get; set; }

        /// <summary>
        /// The ISIN of the <see cref="Share"/>
        /// </summary>
        [PrimaryKey]
        public string ISIN { get; set; }

        /// <summary>
        /// The type of the share (Share, Certificate)
        /// </summary>
        public ShareType ShareType { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Checks the equality of two shares by their ISIN
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            // check if it is a share to compare
            if (obj.GetType() == typeof(Share))
            {
                // compare the ISIN
                return this.ISIN == (obj as Share).ISIN;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

    }

    public enum ShareType
    {
        Share=0,
        Certificate=1,
        ETF=2
    }
}
