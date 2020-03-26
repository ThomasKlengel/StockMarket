using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace StockMarket
{
    /// <summary>
    /// A helper class for Regex expressions.
    /// </summary>
    public static class RegexHelper
    {
        // TODO: default page by isin = https://www.finanzen.net/kurse/de000uf0aa67

        #region Regex strings
        public const string REGEX_Website_Valid1 = "^https:\\/{2}w{3}\\.finanzen\\.net.+$";
        public const string REGEX_Website_Valid2 = "^https:\\/{2}kurse\\.boerse\\.ard\\.de.+$";

        public const string REGEX_SharePrice = "\\d*\\.?\\d*,\\d*";
        public const string REGEX_Group_SharePrice = "\\<tr\\>\\<td class=\"font-bold\"\\>Kurs\\<.*EUR.*\\<span";
        public const string REGEX_Group_IDs = "instrument-id.{150}";
        public const string REGEX_Group_IDs2 = "<title>.*<\\/title>";
        public const string REGEX_Group_IDs3 = "(.{6},\\S{12})";
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
        public const string REGEX_Group_CertPrice2 = "<div .*data-template=\"Bid\".*";

        public const string REGEX_ARD = "<title>.*boerse.ARD.de<\\/title>";
        public const string REGEX_ARD_Group_IDs = "einzelkurs_header"; // get index... substring (index, 1100)
        public const string REGEX_ARD_ISIN = "ISIN \\S{12}";
        public const string REGEX_ARD_WKN = "WKN .{6}";
        public const string REGEX_ARD_Group_ShareName = "<h1>.*h1>";
        public const string REGEX_ARD_Group_SharePrice = "aktueller Wert.{30}";

        #endregion

        /// <summary>
        /// Gets the price of a share from a string.
        /// </summary>
        /// <param name="webContent">the string to check for the price.</param>
        /// <returns>the price of the share.</returns>
        public static double GetSharePrice(string webContent, ShareType type)
        {
            string price = "0.0";
            var ARD = Regex.Match(webContent, REGEX_ARD).Success;
            if (ARD)
            {
                var priceMatch = Regex.Match(webContent, RegexHelper.REGEX_ARD_Group_SharePrice);
                if (!priceMatch.Success)
                {
                    return 0.0;
                }

                // get the SharePrice in the desired format
                price = Regex.Match(priceMatch.Value, RegexHelper.REGEX_SharePrice).Value.Replace(".", string.Empty);
                price = price == string.Empty ? "0.0" : price;
            }
            else
            {
                switch (type)
                {
                    case ShareType.Share:
                        {
                            // get the section of the website which contains the SharePrice
                            // <tr><td class="font-bold">Kurs</td><td colspan="4">18,25 EUR<span
                            var priceMatch = Regex.Match(webContent, RegexHelper.REGEX_Group_SharePrice);
                            if (!priceMatch.Success)
                            {
                                return 0.0;
                            }

                            // get the SharePrice in the desired format
                            price = Regex.Match(priceMatch.Value, RegexHelper.REGEX_SharePrice).Value.Replace(".", string.Empty);
                            break;
                        }

                    case ShareType.Certificate:
                        {
                            // get the current bid price
                            var priceMatch = Regex.Match(webContent, RegexHelper.REGEX_Group_CertPrice);
                            priceMatch = priceMatch.Success ? priceMatch : Regex.Match(webContent, RegexHelper.REGEX_Group_CertPrice2);
                            price = Regex.Match(priceMatch.Value, RegexHelper.REGEX_SharePrice).Value;

                            break;
                        }

                    default: return 0.0;
                }

                price = price == string.Empty ? "0.0" : price;
            }

            return Convert.ToDouble(price, CultureInfo.GetCultureInfo("de-DE"));
        }

        /// <summary>
        /// Tries to get the values defining a share from webcontent
        /// </summary>
        /// <param name="webContent">The input to search for the values</param>
        /// <param name="name">The name of the <see cref="Share"/></param>
        /// <param name="isin">The ISIN of the <see cref="Share"/></param>
        /// <param name="wkn">The WKN of the <see cref="Share"/></param>
        public static void GetShareIDs(string webContent, out string name, out string isin, out string wkn, out double price, string website)
        {
            // set empty values
            wkn = string.Empty; isin = string.Empty; name = string.Empty;
            price = 0;

            var ARD = Regex.Match(webContent, REGEX_ARD).Success;
            if (ARD)
            {
                string ids = string.Empty;
                // get values of WKN and ISIN
                try
                {
                    var idMatch = Regex.Match(webContent, RegexHelper.REGEX_ARD_Group_IDs);
                    ids = webContent.Substring(idMatch.Index, 1100);
                    var wknMatch = Regex.Match(ids, RegexHelper.REGEX_ARD_WKN);
                    var isinMatch = Regex.Match(ids, RegexHelper.REGEX_ARD_ISIN);
                    wkn = wknMatch.Value.Substring(4);
                    isin = isinMatch.Value.Substring(5);
                }
                catch (Exception Ex)
                {
                    MessageBox.Show("Did not find ISIN/WKN match");
                    return;
                }

                // get name of SHARE
                var nameMatch = Regex.Match(ids, RegexHelper.REGEX_ARD_Group_ShareName);
                name = nameMatch.Value.Trim().Replace("h1>", string.Empty).Replace("<", string.Empty).Replace("/", string.Empty);

                //get price of share
                var pricematch = Regex.Match(ids, REGEX_ARD_Group_SharePrice);
                var priceString = Regex.Match(pricematch.Value, REGEX_SharePrice).Value;
                Double.TryParse(priceString, out price);

            }
            else // finanzen.net
            {
                try
                {
                    var type = RegexHelper.GetShareTypeShare(website);
                    // set values if it is a share
                    if (type == ShareType.Share)
                    {
                        // get values of WKN and ISIN
                        try
                        {
                            var idMatch = Regex.Match(webContent, RegexHelper.REGEX_Group_IDs);
                            idMatch = idMatch.Success ? idMatch : Regex.Match(webContent, RegexHelper.REGEX_Group_IDs2);
                            var wknMatch = Regex.Match(idMatch.Value, RegexHelper.REGEX_WKN);
                            var isinMatch = Regex.Match(idMatch.Value, RegexHelper.REGEX_ISIN);
                            if (wknMatch.Success && isinMatch.Success)
                            {
                                wkn = wknMatch.Value.Substring(5);
                                isin = isinMatch.Value.Substring(6);
                            }
                            else
                            {
                                var IDs = Regex.Match(idMatch.Value, RegexHelper.REGEX_Group_IDs3);
                                if (IDs.Success)
                                {
                                    wkn = IDs.Value.Substring(1, 6);
                                    isin = isinMatch.Value.Substring(7, 12);
                                }

                            }
                        }
                        catch (Exception Ex)
                        {
                            MessageBox.Show("Did not find ISIN/WKN match");
                            return;
                        }

                        // < h2 class="box-headline">Aktienkurs Infineon AG in <span id = "jsCurrencySelect" > EUR </ span >
                        // get name of SHARE
                        var nameMatch = Regex.Match(webContent, RegexHelper.REGEX_Group_ShareName);
                        var nameM2 = Regex.Match(nameMatch.Value, RegexHelper.REGEX_ShareName);
                        if (nameM2.Success)
                        {
                            name = nameM2.Value.Substring(10).Trim().Replace(" in", string.Empty);
                        }
                    }

                    // set values if it is a certificate
                    else if (type == ShareType.Certificate)
                    {
                        // get values of WKN and ISIN
                        var title = Regex.Match(webContent, RegexHelper.REGEX_CertificateTitle);
                        var wknMatch = Regex.Match(title.Value, RegexHelper.REGEX_Group_CertWKN);
                        var isinMatch = Regex.Match(title.Value, RegexHelper.REGEX_Group_CertISIN);
                        wkn = wknMatch.Value.Replace("|", string.Empty).Trim();
                        isin = isinMatch.Value.Replace("|", string.Empty).Trim();

                        // get the certificate factor
                        var factorMatch = Regex.Match(title.Value, RegexHelper.REGEX_Group_CertFactor);

                        // get name of SHARE certificate
                        var nameMatch = Regex.Match(title.Value, RegexHelper.REGEX_Group_CertName);
                        name = nameMatch.Value.Substring(4).Replace(" von", string.Empty).Trim() + " Certificate " + factorMatch.Value + "x";
                    }

                }
                catch (Exception Ex)
                {
                    MessageBox.Show("Did not find ISIN/WKN match");
                    return;
                }
            }


        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="share"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public static async Task<double> GetSharePriceAsync(Share share)
        {
            // get the website content
            var content = await WebHelper.GetWebContent(share.WebSite);
            // get the price
            var price = RegexHelper.GetSharePrice(content, share.ShareType);
            if (price == 0.0 && !share.WebSite2.IsNullEmptyWhitespace())
            {
                content = await WebHelper.GetWebContent(share.WebSite2);
                // get the price
                price = RegexHelper.GetSharePrice(content, share.ShareType);
            }

            if (price == 0.0 && !share.WebSite3.IsNullEmptyWhitespace())
            {
                content = await WebHelper.GetWebContent(share.WebSite3);
                // get the price
                price = RegexHelper.GetSharePrice(content, share.ShareType);
            }

            return price;
        }

        /// <summary>
        /// Checks if a website is valid for handling by this programm.
        /// </summary>
        /// <param name="website">the website to check.</param>
        /// <returns>true if the website is valid.</returns>
        public static bool WebsiteIsValid(string website)
        {
            if (website != null)
            {
                if (Regex.Match(website, REGEX_Website_Valid1).Success ||
                        Regex.Match(website, REGEX_Website_Valid2).Success)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if an ISIN is valid for handling by this programm.
        /// </summary>
        /// <param name="website">the ISIN to check.</param>
        /// <returns>true if the ISIN is valid.</returns>
        public static bool IsinIsValid(string isin)
        {
            return Regex.Match(isin, REGEX_ISIN_Valid).Success;
        }

        public static ShareType GetShareTypeShare(string website)
        {
            if (Regex.Match(website, "optionsscheine").Success)
            {
                return ShareType.Certificate;
            }
            if (Regex.Match(website, "knockouts").Success)
            {
                return ShareType.Certificate;
            }
            if (Regex.Match(website, "hebelprodukte").Success)
            {
                return ShareType.Certificate;
            }
            if (Regex.Match(website, "aktien").Success)
            {
                return ShareType.Share;
            }
            if (Regex.Match(website, "kurse").Success)
            {
                return ShareType.Share;
            }

            return ShareType.ETF;
        }
    }
}
