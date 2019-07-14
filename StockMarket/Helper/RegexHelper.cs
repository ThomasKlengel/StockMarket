using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace StockMarket
{
    /// <summary>
    /// A helper class for Regex expressions
    /// </summary>
    public static class RegexHelper
    {
        #region Regex strings
        public const string REGEX_Website_Valid = "^https:\\/{2}w{3}\\.finanzen\\.net.+$";

        public const string REGEX_SharePrice = "\\d*\\.?\\d*,\\d*";
        public const string REGEX_Group_SharePrice = "\\<tr\\>\\<td class=\"font-bold\"\\>Kurs\\<.*EUR.*\\<span";
        public const string REGEX_Group_IDs = "instrument-id\"\\>.{40}";
        public const string REGEX_ISIN = "ISIN: \\S{12}";
        public const string REGEX_WKN = "WKN: .{6}";
        public const string REGEX_Group_ShareName = "box-headline\"\\>Aktienkurs.{50}";
        public const string REGEX_ShareName = "Aktienkurs .* in";
        public const string REGEX_ISIN_Valid = "^\\S{12}$";

        public const string REGEX_CertificateFactor = "\\bFaktor\\b .+ \\bZertifikat\\b";
        public const string REGEX_CertificateTitle = "<title>.*<\\/title>";
        public const string REGEX_Group_CertWKN = ".{6} \\|";
        public const string REGEX_Group_CertISIN = "\\| \\S{12}  *\\|";
        public const string REGEX_Group_CertName = "\\bauf .* von\\b";
        public const string REGEX_Group_CertFactor = "Faktor \\d{1,2}";
        public const string REGEX_Group_CertPrice = "<div .*data-template=\"Bid\".* data-animation.*<\\/span><\\/div>";

        #endregion

        /// <summary>
        /// Gets the price of a share from a string
        /// </summary>
        /// <param name="webContent">the string to check for the price</param>
        /// <returns>the price of the share</returns>
        public static double GetSharePrice(string webContent, ShareType type)
        {
            string price = "";
            switch (type)
            {
                case ShareType.Share:
                    {
                        // get the section of the website which contains the SharePrice
                        //<tr><td class="font-bold">Kurs</td><td colspan="4">18,25 EUR<span
                        var priceMatch = Regex.Match(webContent, RegexHelper.REGEX_Group_SharePrice);
                        if (!priceMatch.Success)
                        {
                            return 0.0;
                        }
                        // get the SharePrice in the desired format
                        price = Regex.Match(priceMatch.Value, RegexHelper.REGEX_SharePrice).Value.Replace(".", "");
                        break;
                    }
                case ShareType.Certificate:
                    {
                        // get the current bid price
                        var priceMath = Regex.Match(webContent, RegexHelper.REGEX_Group_CertPrice);
                        price = Regex.Match(priceMath.Value, RegexHelper.REGEX_SharePrice).Value;

                        break;
                    }
                default: return 0.0;
            }

            return Convert.ToDouble(price, CultureInfo.GetCultureInfo("de-DE"));
        }

        /// <summary>
        /// Checks if a website is valid for handling by this programm
        /// </summary>
        /// <param name="website">the website to check</param>
        /// <returns>true if the website is valid</returns>
        public static bool WebsiteIsValid(string website)
        {
            return Regex.Match(website, REGEX_Website_Valid).Success;
        }

        /// <summary>
        /// Checks if an ISIN is valid for handling by this programm
        /// </summary>
        /// <param name="website">the ISIN to check</param>
        /// <returns>true if the ISIN is valid</returns>
        public static bool IsinIsValid(string isin)
        {
            return Regex.Match(isin, REGEX_ISIN_Valid).Success;
        }

        public static bool IsShareTypeShare(string website)
        {
            if (Regex.Match(website, "aktien").Success)
            {
                return true;
            }
            else if (Regex.Match(website, "optionsscheine").Success)
            {
                return false;
            }
            return true;
        }

    }
}
