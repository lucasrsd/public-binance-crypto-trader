using Binance.Net.Interfaces;
using CriptoTrade.Command.Binance.Balance.Fiat;
using CriptoTrade.Command.Binance.Trades;
using CriptoTrade.Command.Handlers;
using CriptoTrade.Common.Env;
using CriptoTrade.Domain.Exceptions;
using CriptoTrade.Domain.Interface;
using CriptoTrade.Domain.Model.Binance.Trade;
using CriptoTrade.Domain.Model.Core.Ordem;
using CriptoTrade.Domain.Model.Core.Parametros;
using CriptoTrade.Domain.Model.Core.Target;
using CriptoTrade.Domain.Repository.Binance;
using CriptoTrade.Domain.Repository.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinanceNet = Binance.Net;

namespace CriptoTrade.Command.Core.Trades.Buy
{
    public class CriptoBuyCommandHandler : ICommandHandler<CriptoBuyCommandInput>
    {
        private readonly ILogger<CriptoBuyCommandHandler> _logger;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IBinanceClient _binanceClient;
        private readonly ICriptoConfigRepository _configRepository;
        private readonly INotification _notificationService;
        private readonly ICriptoOrdemMonitorRepository _monitorOrdemRepository;

        public CriptoBuyCommandHandler(ILogger<CriptoBuyCommandHandler> logger,
                                        ICommandDispatcher commandDispatcher,
                                        IBinanceClient binanceClient,
                                        ICriptoConfigRepository configRepository,
                                        INotification notificationService,
                                        ICriptoOrdemMonitorRepository monitorOrdemRepository)
        {
            _logger = logger;
            _commandDispatcher = commandDispatcher;
            _binanceClient = binanceClient;
            _configRepository = configRepository;
            _notificationService = notificationService;
            _monitorOrdemRepository = monitorOrdemRepository;
        }

        public async Task<ICommandOutput<CriptoBuyCommandInput>> HandleAsync(CriptoBuyCommandInput input)
        {
            if (input == null || string.IsNullOrEmpty(input.Symbol))
                throw new NullCommandInputException("Order Symbol nao informado");

            if (input.Targets == null || !input.Targets.Any())
                throw new NullCommandInputException("Target nao informado");

            _logger.LogInformation($"Processando compras... {input.Symbol}");
            _logger.LogInformation($"Buscando ultimo trade, synbol: {input.Symbol}");


            var tradeCommand = await _commandDispatcher.DispatchAsync<BinanceTradesCommandInput>(new BinanceTradesCommandInput { Symbol = input.Symbol, Limit = 1 }) as BinanceTradesCommandOutput;

            if (tradeCommand == null || !tradeCommand.Trades.Any())
                throw new NullCommandInputException($"Falha ao obter ultimo trade do symbol {input.Symbol}");

            var trade = tradeCommand.Trades.FirstOrDefault();

            var diferencaTempoTrade = (DateTime.UtcNow - trade.Time.Value).TotalSeconds;

            if (diferencaTempoTrade > 60)
            {
                _logger.LogWarning($"{input.Symbol} Ignorando ultimo trade por ter mais de 60 segundos");
                return new CriptoBuyCommandOutput { };
            }

            var criptoConfig = await _configRepository.GetCriptoConfiguration(EnvVariables.USERNAME, input.Symbol);

            var tradeExecutado = await ProcessarTrade(input.Symbol, trade, input.Targets, criptoConfig);

            return new CriptoBuyCommandOutput { OrdensEnviadas = tradeExecutado != null ? new List<TradeTarget> { tradeExecutado } : null };
        }

        private async Task<TradeTarget> ProcessarTrade(string symbol, TradeModel trade, List<TradeTarget> targets, CriptoConfig criptoConfig)
        {
            if (trade == null)
                throw new Exception("Trade nao pode ser nulo");

            if (trade.Price == null || trade.Quantity == null)
                throw new Exception("Propriedades do trade invalidas, quantidade ou preço");

            var todosDisponiveis = targets;

            if (todosDisponiveis == null || !todosDisponiveis.Any())
            {
                _logger.LogInformation($"Nenhum target disponivel, synbol: {symbol}");
                return null;
            }

            var disponiveis = todosDisponiveis.Where(item => !item.Executado && trade.Price >= item.PrecoAlvoMinimo && trade.Price <= item.PrecoAlvoMaximo).OrderBy(item => item.PrecoAlvoMaximo);

            if (!disponiveis.Any())
            {
                _logger.LogInformation($"Nenhum target para o valor do ultimo trade, symbol: {symbol}, valor ultimo trade: {trade.Price}");
                return null;
            }

            var variacaoMinima = trade.Price * 0.999m;
            var variacaoMaxima = trade.Price * 1.001m;

            var algumComPoucaDiferenca = todosDisponiveis.Where(item => item.Executado && (item.PrecoPago <= variacaoMaxima && item.PrecoPago >= variacaoMinima)).ToList();

            if (algumComPoucaDiferenca.Any())
            {
                _logger.LogInformation($"Ordem com pouca diferenca de preço localizado, symbol: {symbol}, valor ultimo trade: {trade.Price}, variacao minima: {variacaoMinima}, variacao maxima: {variacaoMaxima}");
                return null;
            }

            var alvoOrdem = disponiveis.FirstOrDefault();

            if (criptoConfig.LimiteFiatCripto.HasValue && criptoConfig.LimiteFiatCripto.Value > 0)
            {
                var ordensEmTransitoCompradas = await _monitorOrdemRepository.ObterOrdensPendentesVenda(EnvVariables.USERNAME, symbol);

                if (ordensEmTransitoCompradas != null && ordensEmTransitoCompradas.Any() && ordensEmTransitoCompradas.Any(item => item != null))
                {
                    var totalEmTransito = ordensEmTransitoCompradas.Sum(item => item.PrecoCompra * item.QuantidadeCompra);
                    if (totalEmTransito > criptoConfig.LimiteFiatCripto)
                    {
                        var msgLimite = $"{DateTime.Now} {symbol} - LIMITE COMPRADO EXCEDIDO, LIMITE: {criptoConfig.LimiteFiatCripto} , TOTAL EM TRANSITO: {totalEmTransito}";
                        _logger.LogError(msgLimite);
                        return null;
                    }
                }
            }

            if (alvoOrdem == null) return null;

            if (alvoOrdem.ValorFiatParaCompra <= 0) return null;

            var saldoDisponivel = await _commandDispatcher.DispatchAsync<BinanceBalanceFiatCommandInput>(new BinanceBalanceFiatCommandInput { Asset = "BRL" }) as BinanceBalanceFiatCommandOutput;
            var saldoBrl = saldoDisponivel.Free;

            if (saldoBrl < alvoOrdem.ValorFiatParaCompra) return null;

            alvoOrdem.Executado = true;
            alvoOrdem.PrecoPago = trade.Price.Value;
            alvoOrdem.QuantidadeMoedaComprado = Math.Round(alvoOrdem.ValorFiatParaCompra / trade.Price.Value, criptoConfig.CasasDecimaisCripto, MidpointRounding.ToZero);

            var codigoInternoExchange = Guid.NewGuid().ToString();

            var orderResult = _binanceClient.Spot.Order.PlaceOrder(
                symbol: alvoOrdem.Pair,
                side: BinanceNet.Enums.OrderSide.Buy,
                type: BinanceNet.Enums.OrderType.Limit,
                quantity: alvoOrdem.QuantidadeMoedaComprado,
                orderResponseType: BinanceNet.Enums.OrderResponseType.Full,
                price: alvoOrdem.PrecoPago,
                newClientOrderId: codigoInternoExchange,
                timeInForce: BinanceNet.Enums.TimeInForce.GoodTillCancel);

            var msg = "";

            if (orderResult.Success)
            {
                var monitorOrdem = new MonitorOrdem
                {
                    CreationDate = DateTime.UtcNow,
                    Status = StatusOrdem.Pendente,
                    PrecoCompra = alvoOrdem.PrecoPago,
                    QuantidadeCompra = alvoOrdem.QuantidadeMoedaComprado,
                    CodigoInternoExchange = codigoInternoExchange,
                    OrderBuyId = orderResult.Data.OrderId,
                    Symbol = symbol,
                    Username = EnvVariables.USERNAME
                };

                await _monitorOrdemRepository.Insert(monitorOrdem);

                msg = $"{DateTime.Now} Ordem COMPRA ENVIADA - Retorno Binance: {(orderResult.Success ? "OK" : "NOK")},\n\nOrder Id: {orderResult?.Data?.OrderId}\n\n Symbol:{orderResult?.Data?.Symbol} \n\nTipo: {orderResult?.Data?.Side.ToString()} \n\nStatus: {orderResult?.Data?.Status.ToString()} \n\nPreço: {orderResult?.Data?.Price} \n\nQuantidade: {orderResult?.Data?.Quantity} \n\n Id Interno: {orderResult?.Data?.ClientOrderId}";
                await _notificationService.SendMessage(Common.Env.EnvVariables.Telegram.Groups.BinancePrice, msg);

                return alvoOrdem;
            }
            else
            {
                msg = $"{DateTime.Now} Ordem COMPRA COM ERRO - Retorno Binance: {(orderResult.Success ? "OK" : "NOK")}, Message: {JsonConvert.SerializeObject(orderResult?.Error)}, Json: {JsonConvert.SerializeObject(orderResult?.Data)} - \n\n\n Moeda: {alvoOrdem.Pair} - Quantidade: { alvoOrdem.QuantidadeMoedaComprado}, Preco: {alvoOrdem.PrecoPago}, Id interno: {codigoInternoExchange}, saldo atual moeda brl: {saldoBrl } ";

                var errorMessage = orderResult?.Error?.Message ?? "";

                if (!errorMessage.Contains("Account has insufficient balance", StringComparison.InvariantCultureIgnoreCase))
                    await _notificationService.SendMessage(Common.Env.EnvVariables.Telegram.Groups.BinancePrice, msg);

                return null;
            }
        }
    }
}
