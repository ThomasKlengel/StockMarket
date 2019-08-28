using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StockMarket
{
    /// <summary>
    /// A helper class for Regex expressions
    /// </summary>
    public static class RegexHelper
    {
        //TODO: default page by isin = https://www.finanzen.net/kurse/de000uf0aa67

        //TODO: add regex to get values from other pages like
        //https://kurse.boerse.ard.de/ard/kurse_einzelkurs_uebersicht.htn?i=48310499

        //<!DOCTYPE html>
        //<html lang = "de" >
        //< head >
        //< title > UF0AA6 | DE000UF0AA67 | Amazon.com FaktorZert open end(UBS) aktuell | boerse.ARD.de</title>
        //<meta http-equiv="X-UA-Compatible" content="IE=edge"/>
        //<meta name = "language" content="de" />
        //<meta name = "apple-mobile-web-app-capable" content="yes"/>
        //<meta name = "apple-mobile-web-app-status-bar-style" content="black-translucent"/>
        //<meta name = "viewport" content="width=device-width"/>
        //<meta name = "description" content="Finden Sie Informationen zum Zertifikat Amazon.com FaktorZert  open end (UBS) (WKN UF0AA6, ISIN DE000UF0AA67), sowie den aktuellen Zertifikat-Kurs und Chart." />


        //<table summary = "Die folgende Tabelle enth&auml;lt Kursinformationen zu Amazon.com FaktorZert  open end (UBS)." cellspacing="0" >
        //  <tbody>
        //    <tr class="gray_bg">
        //      <th id = "aktueller_kurs" scope="row" class="tleft"><strong>Aktueller Kurs:</strong></th>
        //      <td headers = "aktueller_kurs" class="tright">5,39&nbsp;&euro;</td>
        //    </tr>
        //    <tr class="">
        //      <th id = "tageshoch" scope="row" class="tleft"><strong>Tageshoch:</strong></th>
        //      <td headers = "tageshoch" class="tright">5,39&nbsp;&euro;</td>
        //    </tr>
        //    <tr class="gray_bg">
        //      <th id = "tagestief" scope="row" class="tleft"><strong>Tagestief:</strong></th>
        //      <td headers = "tagestief" class="tright">5,21&nbsp;&euro;</td>
        //    </tr>
        //    <tr class="">
        //      <th id = "eroeffnung" scope="row" class="tleft"><strong>Er&ouml;ffnung:</strong></th>
        //      <td headers = "eroeffnung" class="tright">5,26&nbsp;&euro;</td>
        //    </tr>
        //    <tr class="gray_bg">
        //      <th id = "vortag" scope="row" class="tleft"><strong>Vortag:</strong> (16.08.19)</th>
        //      <td headers = "vortag" class="tright">5,06&nbsp;&euro;</td>
        //    </tr>
        //    <tr class="">
        //      <th id = "wochenhoch" scope="row" class="tleft"><strong>52-Wochenhoch:</strong></th>
        //      <td headers = "wochenhoch" class="tright">9,92&nbsp;&euro;</td>
        //    </tr>
        //    <tr class="gray_bg">
        //      <th id = "wochentief" scope="row" class="tleft"><strong>52-Wochentief:</strong></th>
        //      <td headers = "wochentief" class="tright">3,55&nbsp;&euro;</td>
        //    </tr>
        //    <tr class="">
        //      <th id = "boerse" scope="row" class="tleft"><strong>Börsenplatz:</strong></th>
        //      <td headers = "boerse" class="tright">Stuttgart</td>
        //    </tr>

        //    <tr class="gray_bg">
        //      <th id = "gattung" scope="row" class="tleft"><strong>Gattung:</strong></th>
        //      <td headers = "gattung" class="tright">Faktor</td>
        //    </tr>

        //      <tr class="">
        //        <th id = "emittent" scope="row" class="tleft"><strong>Emittent:</strong></th>
        //        <td headers = "boerse" class="tright">UBS</td>
        //      </tr>

        //  </tbody>
        //</table>



        #region Regex strings
        public const string REGEX_Website_Valid1 = "^https:\\/{2}w{3}\\.finanzen\\.net.+$";
        public const string REGEX_Website_Valid2 = "^https:\\/{2}kurse\\.boerse\\.ard\\.de.+$";

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

        public const string REGEX_ARD = "<title>.*boerse.ARD.de<\\/title>";
        public const string REGEX_Group_ARD_Price = "<td headers=\"aktueller_kurs.*<\\/td>"; // headers = \"aktueller_kurs.*<\\/td>

        #endregion

        /// <summary>
        /// Gets the price of a share from a string
        /// </summary>
        /// <param name="webContent">the string to check for the price</param>
        /// <returns>the price of the share</returns>
        public static double GetSharePrice(string webContent, ShareType type)
        {
            string price = "0.0";
            var ARD = Regex.Match(webContent, REGEX_ARD).Success;
            if (ARD)
            {
                var priceMatch = Regex.Match(webContent, RegexHelper.REGEX_Group_ARD_Price);
                if (!priceMatch.Success)
                {
                    return 0.0;
                }
                // get the SharePrice in the desired format
                price = Regex.Match(priceMatch.Value, RegexHelper.REGEX_SharePrice).Value.Replace(".", "");
                price = price == string.Empty ? "0.0" : price;
            }
            else
            {
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
                price = price == string.Empty ? "0.0" : price;
            }
            return Convert.ToDouble(price, CultureInfo.GetCultureInfo("de-DE"));

        }

        public static async Task<double> GetSharePriceAsync(Share share)
        {
            // get the website content
            var content = await WebHelper.getWebContent(share.WebSite);
            //get the price
            var price = RegexHelper.GetSharePrice(content, share.ShareType);
            if (price == 0.0 && share.WebSite2!= string.Empty)
            {
                content = await WebHelper.getWebContent(share.WebSite2);
                //get the price
                price = RegexHelper.GetSharePrice(content, share.ShareType);
            }
            if (price == 0.0 && share.WebSite3 != string.Empty)
            {
                content = await WebHelper.getWebContent(share.WebSite3);
                //get the price
                price = RegexHelper.GetSharePrice(content, share.ShareType);
            }

            return price;

        }

        /// <summary>
        /// Checks if a website is valid for handling by this programm
        /// </summary>
        /// <param name="website">the website to check</param>
        /// <returns>true if the website is valid</returns>
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
