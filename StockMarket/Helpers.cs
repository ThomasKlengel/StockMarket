using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarket
{    
    public static class Helpers
    {
        public const string REGEX_SharePrice = "\\d*\\.?\\d*,\\d*";
        public const string REGEX_Group_SharePrice = "\\<tr\\>\\<td class=\"font-bold\"\\>Kurs\\<.*EUR.*\\<span";
        public const string REGEX_Group_IDs = "instrument-id\"\\>.{40}";
        public const string REGEX_ISIN = "ISIN: \\S{2}\\d{10}";
        public const string REGEX_WKN = "WKN: \\d{6}";
        public const string REGEX_Group_ShareName = "box-headline\"\\>Aktienkurs.{50}";
        public const string REGEX_ShareName = "Aktienkurs .* in";

    }
}
