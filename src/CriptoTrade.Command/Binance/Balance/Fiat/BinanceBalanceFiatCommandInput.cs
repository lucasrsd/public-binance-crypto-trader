using CriptoTrade.Command.Handlers;
using System;
using System.Collections.Generic;
using System.Text;

namespace CriptoTrade.Command.Binance.Balance.Fiat
{
    public class BinanceBalanceFiatCommandInput : ICommandInput
    {
        public string Asset { get; set; }
    }
}
