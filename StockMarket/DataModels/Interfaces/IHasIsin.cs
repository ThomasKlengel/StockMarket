﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMarket
{
    /// <summary>
    /// An interface representing any share component that relates to an isin.
    /// </summary>
    public interface IHasIsin
    {
        string ISIN { get; }
    }
}
