using CriptoTrade.Command.Handlers;
using CriptoTrade.Domain.Model.Binance.Trade;
using System;
using System.Collections.Generic;
using System.Text;

namespace CriptoTrade.Command.Binance.Trades
{
    public class BinanceTradesCommandOutput : ICommandOutput<BinanceTradesCommandInput>
    {
        public List<TradeModel> Trades { get; set; }
    }
}
