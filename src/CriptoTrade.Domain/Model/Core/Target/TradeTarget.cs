using CriptoTrade.Common.Env;
using System;
using System.Collections.Generic;
using System.Text;

namespace CriptoTrade.Domain.Model.Core.Target
{
    public class TradeTarget
    {
        public string Id { get; set; }

        public string Pair { get; set; }

        public decimal PrecoAlvoMinimo { get; set; }

        public decimal PrecoAlvoMaximo { get; set; }

        public decimal PrecoPago { get; set; }

        public decimal ValorFiatParaCompra { get; set; }
        public decimal QuantidadeMoedaComprado { get; set; }

        public bool Executado { get; set; }

        public DateTime? DataCriacao { get; set; }
    }
}
