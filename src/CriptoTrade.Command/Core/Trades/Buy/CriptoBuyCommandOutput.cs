using CriptoTrade.Command.Handlers;
using CriptoTrade.Domain.Model.Core.Target;
using System.Collections.Generic;

namespace CriptoTrade.Command.Core.Trades.Buy
{
    public class CriptoBuyCommandOutput : ICommandOutput<CriptoBuyCommandInput>
    {
        public List<TradeTarget> OrdensEnviadas { get; set; }
    }
}
