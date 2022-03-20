using System;
using System.Collections.Generic;
using System.Text;

namespace CriptoTrade.Domain.Model.Binance.Trade
{
   public class TradeModel
    {
        public long? Id { get; set; }
        public decimal? Price { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? QuoteQuantity { get; set; }
        public DateTime? Time { get; set; }
        public bool? IsBuyerMaker { get; set; }
        public bool? IsBestMatch { get; set; }
    }
}
