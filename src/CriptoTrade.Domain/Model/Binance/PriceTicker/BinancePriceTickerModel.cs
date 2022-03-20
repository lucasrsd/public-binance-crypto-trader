using System;
using System.Collections.Generic;
using System.Text;

namespace CriptoTrade.Domain.Model.Binance.PriceTicker
{
    public class BinancePriceTickerModel
    {
        public string Symbol { get; set; }
        public decimal? Price { get; set; }
        public DateTime? PriceDate { get; set; }
        public string Teste { get; set; }
    }
}
