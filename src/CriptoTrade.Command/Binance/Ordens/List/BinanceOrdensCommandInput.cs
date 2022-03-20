using CriptoTrade.Command.Handlers;
using System;
using System.Collections.Generic;
using System.Text;

namespace CriptoTrade.Command.Binance.Ordens.List
{
    public class BinanceOrdensCommandInput : ICommandInput
    {
        public string Symbol { get; set; } 
    }
}
