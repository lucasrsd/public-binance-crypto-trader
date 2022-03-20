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

namespace CriptoTrade.Command.Core.Ordem.Monitor.Compras
{
    public class OrdemMonitorComprasCommandHandler : ICommandHandler<OrdemMonitorComprasCommandInput>
    {
        private readonly ILogger<OrdemMonitorComprasCommandHandler> _logger;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IBinanceClient _binanceClient;
        private readonly ICriptoConfigRepository _configRepository;
        private readonly INotification _notificationService;
        private readonly ICriptoOrdemMonitorRepository _monitorOrdemRepository;

        public OrdemMonitorComprasCommandHandler(ILogger<OrdemMonitorComprasCommandHandler> logger,
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

        public async Task<ICommandOutput<OrdemMonitorComprasCommandInput>> HandleAsync(OrdemMonitorComprasCommandInput input)
        {
            _logger.LogInformation($"Buscando ordens pendentes e compradas para input de venda");

            await ProcessarOrdensCompradas();

            return new OrdemMonitorComprasCommandOutput { };
        }

        private async Task ProcessarOrdensCompradas()
        {
            var openOrders = await _monitorOrdemRepository.GetByStatus(EnvVariables.USERNAME, new List<StatusOrdem> { StatusOrdem.Pendente });

            if (openOrders == null || !openOrders.Any())
            {
                _logger.LogInformation($"Nenhuma ordem pendente");
                return;
            }

            var moedasAbertas = openOrders.Select(item => item.Symbol).Distinct();

            foreach (var symbol in moedasAbertas)
                await ProcessarCompraPorSymbol(symbol, openOrders.Where(item => item.Symbol.Equals(symbol)).ToList());
        }

        private async Task ProcessarCompraPorSymbol(string symbol, List<MonitorOrdem> ordens)
        {
            var criptoConfig = await _configRepository.GetCriptoConfiguration(EnvVariables.USERNAME, symbol);

            if (criptoConfig == null)
            {
                _logger.LogError($"Cripto config nao encontrado {symbol}");
                return;
            }

            var ordensHojeRequest = _binanceClient.Spot.Order.GetAllOrders(symbol, startTime: DateTime.UtcNow.AddHours(-23), endTime: DateTime.UtcNow);

            if (!ordensHojeRequest.Success)
            {
                _logger.LogError($"Erro conciliando ordens abertas para moeda: {symbol}");
                return;
            }

            var ordensHoje = ordensHojeRequest.Data.Where(item => item.Side == BinanceNet.Enums.OrderSide.Buy).ToList();

            foreach (var ordem in ordens)
            {
                var binanceOrder = ordensHoje.Where(item => item.OrderId.Equals(ordem.OrderBuyId)).FirstOrDefault();

                if (binanceOrder == null)
                {
                    var binanceOrderRequest = _binanceClient.Spot.Order.GetOrder(symbol, orderId: ordem.OrderBuyId);

                    if (!binanceOrderRequest.Success)
                    {
                        _logger.LogInformation($"Ordem pendente {ordem.OrderBuyId} não localizada na binance");
                        continue;
                    }

                    if (binanceOrderRequest.Data.Side != BinanceNet.Enums.OrderSide.Buy)
                    {
                        _logger.LogInformation($"Ordem {ordem.OrderBuyId} com side diferente de BUY, ignorando ");
                        continue;
                    }

                    binanceOrder = binanceOrderRequest.Data;
                }

                if (binanceOrder.Status == BinanceNet.Enums.OrderStatus.Filled)
                {
                    ordem.UltimaAtualizacao = DateTime.UtcNow;
                    ordem.Status = StatusOrdem.CompraConfirmada;

                    await _monitorOrdemRepository.Update(ordem);

                    var saldoCriptoDisponivelCommand = await _commandDispatcher.DispatchAsync<BinanceBalanceFiatCommandInput>(new BinanceBalanceFiatCommandInput { Asset = criptoConfig.CriptoSymbol }) as BinanceBalanceFiatCommandOutput;

                    var saldoDisponivelCripto = saldoCriptoDisponivelCommand.Free;

                    var quantidadeVenda = Math.Round(binanceOrder.Quantity, criptoConfig.CasasDecimaisCripto);

                    if (saldoDisponivelCripto < quantidadeVenda)
                        quantidadeVenda = Math.Round(saldoDisponivelCripto, criptoConfig.CasasDecimaisCripto, MidpointRounding.ToZero);

                    var valorVenda = Math.Round(binanceOrder.Price * EnvVariables.Trade.VALORIZACAO_PARA_VENDER, criptoConfig.CasasDecimaisFiat);

                    var orderClientId = $"{ordem._id}";

                    var orderResult = _binanceClient.Spot.Order.PlaceOrder(
                       symbol: symbol,
                       side: BinanceNet.Enums.OrderSide.Sell,
                       type: BinanceNet.Enums.OrderType.Limit,
                       quantity: quantidadeVenda,
                       orderResponseType: BinanceNet.Enums.OrderResponseType.Full,
                       price: valorVenda,
                       newClientOrderId: orderClientId,
                       timeInForce: BinanceNet.Enums.TimeInForce.GoodTillCancel);

                    var msg = "";

                    if (!orderResult.Success)
                    {
                        ordem.UltimaAtualizacao = DateTime.UtcNow;
                        ordem.Status = StatusOrdem.Erro;

                        await _monitorOrdemRepository.Update(ordem);

                        msg = $"{DateTime.Now} Ordem VENDA COM ERRO - Retorno Binance: {(orderResult.Success ? "OK" : "NOK")}, Message: {JsonConvert.SerializeObject(orderResult?.Error)}, Json: {JsonConvert.SerializeObject(orderResult?.Data)} \n\n\n Order Internal Id: {ordem._id.ToString()} - \n\n Order Id: {ordem.OrderBuyId} \n\n  Moeda: {symbol} - Quantidade: {quantidadeVenda}, Preco: {valorVenda}, Id interno: {orderClientId}, saldo atual moeda: {saldoDisponivelCripto}";
                        await _notificationService.SendMessage(EnvVariables.Telegram.Groups.BinancePrice, msg);
                        return;
                    }

                    ordem.OrderSellId = orderResult.Data.OrderId;
                    ordem.UltimaAtualizacao = DateTime.UtcNow;
                    ordem.Status = StatusOrdem.VendaCriada;
                    ordem.PrecoVenda = valorVenda;
                    ordem.PrecoCompra = binanceOrder.AverageFillPrice; // Atribui preco da compra para preco medio
                    ordem.ValorSpread = ordem.CalcularValorSpread();
                    ordem.PercentualSpread = ordem.CalcularPercentualSpread();

                    await _monitorOrdemRepository.Update(ordem);

                    msg = $"{DateTime.Now} Ordem VENDA ENVIADA - Retorno Binance: {(orderResult.Success ? "OK" : "NOK")},\n\nID ORDEM VENDA: {orderResult?.Data?.OrderId}\n\n ID ORDEM COMPRA: {ordem.OrderBuyId} \n\n Symbol:{orderResult?.Data?.Symbol} \n\nTipo: {orderResult?.Data?.Side.ToString()} \n\n Order Internal Id: {ordem._id.ToString()} \n\n Status: {orderResult?.Data?.Status.ToString()} \n\nPreço Ordem: {orderResult?.Data?.Price} \n\nPreço Medio: {ordem.PrecoCompra} \n\nQuantidade: {orderResult?.Data?.Quantity} \n\n Valor Spread: {ordem.ValorSpread} \n\n Percentual Spread: {ordem.PercentualSpread}  \n\n Id Referencia ordem binance: {orderResult?.Data?.ClientOrderId}";

                    await _notificationService.SendMessage(EnvVariables.Telegram.Groups.BinancePrice, msg);
                }
            }
        }
    }
}
