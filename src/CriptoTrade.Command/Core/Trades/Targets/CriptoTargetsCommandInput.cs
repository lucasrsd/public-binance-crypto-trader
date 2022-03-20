using CriptoTrade.Command.Handlers;
using CriptoTrade.Domain.Model.Binance.Trade; 
using System.Collections.Generic;

namespace CriptoTrade.Command.Core.Trades.Targets
{
    public class CriptoTargetsCommandInput : ICommandInput
    {
        public string Symbol { get; set; }
    }
}
