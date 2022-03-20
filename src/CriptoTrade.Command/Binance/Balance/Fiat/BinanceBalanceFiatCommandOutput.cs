using Binance.Net.Objects.Spot.SpotData;
using CriptoTrade.Command.Handlers;
using CriptoTrade.Domain.Model.Binance.Trade;
using System;
using System.Collections.Generic;
using System.Text;

namespace CriptoTrade.Command.Binance.Balance.Fiat
{
    public class BinanceBalanceFiatCommandOutput : ICommandOutput<BinanceBalanceFiatCommandInput>
    {
        public string Asset { get; set; }
        public decimal Total { get; set; }
        public decimal Locked { get; set; }
        public decimal Free { get; set; }
    }
}
