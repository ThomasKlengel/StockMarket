using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarket
{
    public static class GeneralHelper
    {
        public static bool IsNullEmptyWhitespace(this string s)
        {
            return (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s));
        }
    }
}
