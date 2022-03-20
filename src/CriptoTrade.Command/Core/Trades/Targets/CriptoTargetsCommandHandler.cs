using CriptoTrade.Command.Binance.Ordens.Cancelar;
using CriptoTrade.Command.Binance.Ordens.List;
using CriptoTrade.Command.Binance.Price.Ticker24h;
using CriptoTrade.Command.Handlers;
using CriptoTrade.Common.Env;
using CriptoTrade.Domain.Exceptions;
using CriptoTrade.Domain.Model.Core.Target;
using CriptoTrade.Domain.Repository.Binance;
using CriptoTrade.Domain.Repository.Core;
using CriptoTrade.Mapper.Binance;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinanceNet = Binance.Net;

namespace CriptoTrade.Command.Core.Trades.Targets
{
    public class CriptoTargetsCommandHandler : ICommandHandler<CriptoTargetsCommandInput>
    {
        private readonly ILogger<CriptoTargetsCommandHandler> _logger;
        private readonly ICommandDispatcher _dispatcher;
        private readonly ICriptoConfigRepository _criptoConfigRepository;
        private readonly ICriptoOrdemMonitorRepository _criptoOrdemMonitor;

        public CriptoTargetsCommandHandler(ILogger<CriptoTargetsCommandHandler> logger,
                                            ICommandDispatcher dispatcher,
                                            ICriptoConfigRepository criptoConfigRepository,
                                            ICriptoOrdemMonitorRepository criptoOrdemMonitor)
        {
            _logger = logger;
            _dispatcher = dispatcher;
            _criptoConfigRepository = criptoConfigRepository;
            _criptoOrdemMonitor = criptoOrdemMonitor;
        }

        public async Task<ICommandOutput<CriptoTargetsCommandInput>> HandleAsync(CriptoTargetsCommandInput input)
        {
            if (input == null || string.IsNullOrEmpty(input.Symbol))
                throw new NullCommandInputException("Order  Symbol nao informado");

            _logger.LogInformation($"Compilando targets symbol: {input.Symbol}");

            var criptoConfig = await _criptoConfigRepository.GetCriptoConfiguration(EnvVariables.USERNAME, input.Symbol);

            if (criptoConfig == null)
                throw new Exception($"Configuracoes para moeda ({input.Symbol}) nao cadastradas.");

            var price24hCommand = await _dispatcher.DispatchAsync<BinancePrice24hCommandInput>(new BinancePrice24hCommandInput { Symbol = input.Symbol }) as BinancePrice24hCommandOutput;
            var price24h = price24hCommand.Price;

            var currentPrice = price24h?.WeightedAveragePrice ?? 0;

            var faixaValorizacao = Math.Round(currentPrice * EnvVariables.Trade.FAIXA_VALORIZACAO, criptoConfig.CasasDecimaisFiat);

            // Calcula 40% da variacao minima e maxima, alocando metade para limite maximo e metade para limite minimo
            var diferencaMaiorEMenor = ((price24h.HighPrice - price24h.LowPrice) * 0.5m) / 2;

            // Aumentando limite minimo de compra
            var minimo = currentPrice - (diferencaMaiorEMenor * 2);
            var maximo = currentPrice + diferencaMaiorEMenor;

            _logger.LogInformation($"{input.Symbol} Faixa compilada - Valor faixa: {faixaValorizacao}, Minimo: {minimo}, Maximo: {maximo}");

            var openOrdersCommand = await _dispatcher.DispatchAsync<BinanceOrdensCommandInput>(new BinanceOrdensCommandInput { Symbol = input.Symbol }) as BinanceOrdensCommandOutput;
            var openOrders = openOrdersCommand.OrdensAbertas;

            // Validar horarios BR e GMT
            var ordensAbertasHaMaisTempoParaCancelar = openOrders.Where(item => item.Side == BinanceNet.Enums.OrderSide.Buy &&
                                                                            item.Status == BinanceNet.Enums.OrderStatus.New &&
                                                                            item.CreateTime < DateTime.UtcNow.AddSeconds(EnvVariables.Trade.TEMPO_SEGUNDOS_CANCELAR_ORDEM_COMPRA_NAO_EXECUTADA)).ToList();

            if (ordensAbertasHaMaisTempoParaCancelar != null && ordensAbertasHaMaisTempoParaCancelar.Any())
            {
                foreach (var ordemACancelar in ordensAbertasHaMaisTempoParaCancelar)
                {
                    var cancelResult = await _dispatcher.DispatchAsync<BinanceCancelarOrdemCommandInput>(new BinanceCancelarOrdemCommandInput { Symbol = input.Symbol, OrderId = ordemACancelar.OrderId }) as BinanceCancelarOrdemCommandOutput;
                }

                openOrdersCommand = await _dispatcher.DispatchAsync<BinanceOrdensCommandInput>(new BinanceOrdensCommandInput { Symbol = input.Symbol }) as BinanceOrdensCommandOutput;
                openOrders = openOrdersCommand.OrdensAbertas;
            }

            var newTargets = new List<TradeTarget>();

            var ordensComposicaoDisponiveis = new List<OrdensComposicao>();
            if (openOrders != null)
                ordensComposicaoDisponiveis.AddRange(openOrders.Select(item => new OrdensComposicao { Preco = item.Price, Quantidade = item.Quantity }));

            var ordensPendentesVenda = await _criptoOrdemMonitor.ObterOrdensPendentesVenda(EnvVariables.USERNAME, input.Symbol);
            if (ordensPendentesVenda != null)
                ordensComposicaoDisponiveis.AddRange(ordensPendentesVenda.Select(item => new OrdensComposicao { Preco = item.PrecoCompra ?? 0, Quantidade = item.QuantidadeCompra ?? 0 }));


            for (decimal v = minimo; v <= maximo; v += faixaValorizacao)
            {
                var valorMinimo = v - faixaValorizacao;
                var valorMaximo = v;

                var maiorOrdemNaFaixa = ordensComposicaoDisponiveis.Where(item => item.Preco >= valorMinimo && item.Preco <= valorMaximo).OrderByDescending(item => item.Quantidade).FirstOrDefault();

                var valorAporte = criptoConfig.ValorAporte;

                if (maiorOrdemNaFaixa != null)
                {
                    _logger.LogInformation($"Faixa sendo criada como executada pendente liquidacao, ja existe uma ordem de compra dentro do range, moeda: {input.Symbol}, faixa considerada: {valorMinimo} - {valorMaximo}");

                    var target = new TradeTarget
                    {
                        Id = Guid.NewGuid().ToString(),
                        DataCriacao = DateTime.Now,
                        Pair = input.Symbol,
                        ValorFiatParaCompra = valorAporte,
                        Executado = true,
                        PrecoAlvoMinimo = valorMinimo,
                        PrecoAlvoMaximo = valorMaximo,
                        PrecoPago = maiorOrdemNaFaixa.Preco,
                        QuantidadeMoedaComprado = maiorOrdemNaFaixa.Quantidade
                    };

                    newTargets.Add(target);
                }
                else
                {
                    var target = new TradeTarget
                    {
                        Id = Guid.NewGuid().ToString(),
                        DataCriacao = DateTime.Now,
                        Pair = input.Symbol,
                        ValorFiatParaCompra = valorAporte,
                        Executado = false,
                        PrecoAlvoMinimo = valorMinimo,
                        PrecoAlvoMaximo = valorMaximo
                    };

                    newTargets.Add(target);
                }
            }

            return new CriptoTargetsCommandOutput { Targets = newTargets };
        }
    }

    internal class OrdensComposicao
    {
        public decimal Preco { get; set; }
        public decimal Quantidade { get; set; }
    }
}
