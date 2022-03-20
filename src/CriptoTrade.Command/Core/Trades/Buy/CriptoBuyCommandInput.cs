using CriptoTrade.Command.Handlers;
using CriptoTrade.Domain.Model.Binance.Trade;
using CriptoTrade.Domain.Model.Core.Target;
using System.Collections.Generic;

namespace CriptoTrade.Command.Core.Trades.Buy
{
    public class CriptoBuyCommandInput : ICommandInput
    {
        public string Symbol { get; set; }
        public List<TradeTarget> Targets { get; set; }
    }
}
