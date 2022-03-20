using CriptoTrade.Command.Handlers;
using System;
using System.Collections.Generic;
using System.Text;

namespace CriptoTrade.Command.Binance.Ordens.Cancelar
{
    public class BinanceCancelarOrdemCommandInput : ICommandInput
    {
        public string Symbol { get; set; }
        public long? OrderId { get; set; }
    }
}
