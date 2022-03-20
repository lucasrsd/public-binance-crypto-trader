using CriptoTrade.Common.Env;
using CriptoTrade.Domain.Model.BaseRepository;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace CriptoTrade.Domain.Model.Core.Ordem
{
    [BsonIgnoreExtraElements]
    public class MonitorOrdem : BaseRepositoryEntity
    {
        public string Username { get; set; }
        public long OrderBuyId { get; set; }
        public long OrderSellId { get; set; }
        public string Symbol { get; set; }
        public string CodigoInternoExchange { get; set; }
        public DateTime? CreationDate { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [BsonRepresentation(BsonType.String)]
        public StatusOrdem Status { get; set; }
        public DateTime? UltimaAtualizacao { get; set; }

        public decimal? QuantidadeCompra { get; set; }
        public decimal? PrecoCompra { get; set; }

        public decimal? PrecoVenda { get; set; }

        public decimal? PercentualSpread { get; set; }
        public decimal? ValorSpread { get; set; }

        private decimal ObterValorTaxa(decimal value)
        {
            return value * EnvVariables.Trade.TAXA_TRADE;
        }

        public decimal CalcularValorSpread()
        {
            if (PrecoCompra.HasValue && PrecoVenda.HasValue && QuantidadeCompra.HasValue)
            {
                if (PrecoCompra.Value > 0 && PrecoVenda.Value > 0 && QuantidadeCompra.Value > 0)
                {
                    var valorVenda = PrecoVenda.Value * QuantidadeCompra.Value;
                    var valorCompra = PrecoCompra.Value * QuantidadeCompra.Value;

                    var taxaVenda = ObterValorTaxa(valorVenda);
                    var taxaCompra = ObterValorTaxa(valorCompra);

                    return Math.Round((valorVenda - valorCompra) - (taxaVenda) - (taxaCompra), 6);
                }
            }

            return 0;
        }

        public decimal CalcularPercentualSpread()
        {
            if (PrecoCompra.HasValue && PrecoCompra.Value > 0)
            {
                var valorSpread = CalcularValorSpread();

                if (valorSpread == 0) return 0;

                return Math.Round(valorSpread / (PrecoCompra.Value * QuantidadeCompra.Value) * 100, 6);
            }

            return 0;
        }
    }
}
