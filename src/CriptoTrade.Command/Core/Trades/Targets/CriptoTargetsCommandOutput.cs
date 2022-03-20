using CriptoTrade.Command.Handlers;
using CriptoTrade.Domain.Model.Core.Target;
using System.Collections.Generic;

namespace CriptoTrade.Command.Core.Trades.Targets
{
    public class CriptoTargetsCommandOutput : ICommandOutput<CriptoTargetsCommandInput>
    {
        public List<TradeTarget> Targets { get; set; }
    }
}
