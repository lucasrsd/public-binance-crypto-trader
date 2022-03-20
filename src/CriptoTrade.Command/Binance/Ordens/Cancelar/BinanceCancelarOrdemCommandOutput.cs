using Binance.Net.Objects.Spot.SpotData;
using CriptoTrade.Command.Handlers;
using CriptoTrade.Domain.Model.Binance.Trade;
using System;
using System.Collections.Generic;
using System.Text;

namespace CriptoTrade.Command.Binance.Ordens.Cancelar
{
    public class BinanceCancelarOrdemCommandOutput : ICommandOutput<BinanceCancelarOrdemCommandInput>
    {
        public List<BinanceOrder> OrdensAbertas { get; set; }
    }
}
