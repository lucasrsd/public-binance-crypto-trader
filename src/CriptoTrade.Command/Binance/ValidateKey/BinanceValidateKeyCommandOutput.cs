using CriptoTrade.Command.Handlers;
using CriptoTrade.Domain.Model.Binance.Trade;
using System;
using System.Collections.Generic;
using System.Text;

namespace CriptoTrade.Command.Binance.ValidateKey
{
    public class BinanceValidateKeyCommandOutput : ICommandOutput<BinanceValidateKeyCommandInput>
    {
        public bool Success { get; set; }
    }
}
