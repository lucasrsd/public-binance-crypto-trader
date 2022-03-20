using CriptoTrade.Command.Handlers;
using System;
using System.Collections.Generic;
using System.Text;

namespace CriptoTrade.Command.Binance.Trades
{
    public class BinanceTradesCommandInput : ICommandInput
    {
        public string Symbol { get; set; }
        public int? Limit { get; set; }
    }
}
