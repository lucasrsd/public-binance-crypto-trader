using Binance.Net.Objects.Spot.SpotData;
using CriptoTrade.Command.Handlers;
using CriptoTrade.Domain.Model.Binance.Trade;
using System;
using System.Collections.Generic;
using System.Text;

namespace CriptoTrade.Command.Binance.Ordens.List
{
    public class BinanceOrdensCommandOutput : ICommandOutput<BinanceOrdensCommandInput>
    {
        public List<BinanceOrder> OrdensAbertas { get; set; }

        internal object Where(Func<object, bool> p)
        {
            throw new NotImplementedException();
        }
    }
}
