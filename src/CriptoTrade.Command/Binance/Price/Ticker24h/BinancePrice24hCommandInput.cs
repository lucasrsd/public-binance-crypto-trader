using CriptoTrade.Command.Handlers;
using System;
using System.Collections.Generic;
using System.Text;

namespace CriptoTrade.Command.Binance.Price.Ticker24h
{
    public class BinancePrice24hCommandInput : ICommandInput
    {
        public string Symbol { get; set; } 
    }
}
