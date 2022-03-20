using Binance.Net.Interfaces;
using CriptoTrade.Command.Handlers;
using CriptoTrade.Domain.Model.Binance.Trade;
using System;
using System.Collections.Generic;
using System.Text;

namespace CriptoTrade.Command.Binance.Price.Ticker24h
{
    public class BinancePrice24hCommandOutput : ICommandOutput<BinancePrice24hCommandInput>
    {
        public IBinanceTick Price { get; set; }
    }
}
