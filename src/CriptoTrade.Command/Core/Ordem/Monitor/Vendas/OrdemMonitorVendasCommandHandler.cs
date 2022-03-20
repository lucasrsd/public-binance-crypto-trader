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

namespace CriptoTrade.Command.Core.Ordem.Monitor.Vendas
{
    public class OrdemMonitorVendasCommandHandler : ICommandHandler<OrdemMonitorVendasCommandInput>
    {
        private readonly ILogger<OrdemMonitorVendasCommandHandler> _logger;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IBinanceClient _binanceClient;
        private readonly ICriptoConfigRepository _configRepository;
        private readonly INotification _notificationService;
        private readonly ICriptoOrdemMonitorRepository _monitorOrdemRepository;

        public OrdemMonitorVendasCommandHandler(ILogger<OrdemMonitorVendasCommandHandler> logger,
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

        public async Task<ICommandOutput<OrdemMonitorVendasCommandInput>> HandleAsync(OrdemMonitorVendasCommandInput input)
        {
            _logger.LogInformation($"Buscando ordens pendentes e compradas para input de venda");

            await ProcessarOrdensVendidas();

            return new OrdemMonitorVendasCommandOutput { };
        }

        private async Task ProcessarOrdensVendidas()
        {
            var openOrders = await _monitorOrdemRepository.GetByStatus(EnvVariables.USERNAME, new List<StatusOrdem> { StatusOrdem.VendaCriada });

            if (openOrders == null || !openOrders.Any())
            {
                _logger.LogInformation($"Nenhuma ordem pendente");
                return;
            }

            var moedasAbertas = openOrders.Select(item => item.Symbol).Distinct();

            foreach (var symbol in moedasAbertas)
                await ProcessarOrdemVendidaPorSymbol(symbol, openOrders.Where(item => item.Symbol.Equals(symbol)).ToList());
        }

        private async Task ProcessarOrdemVendidaPorSymbol(string symbol, List<MonitorOrdem> ordens)
        {
            var ordensHojeRequest = _binanceClient.Spot.Order.GetAllOrders(symbol, startTime: DateTime.UtcNow.AddHours(-23), endTime: DateTime.UtcNow);

            if (!ordensHojeRequest.Success)
            {
                _logger.LogError($"Erro conciliando ordens de venda abertas para moeda: {symbol} {JsonConvert.SerializeObject(ordensHojeRequest?.Error)}");
                return;
            }

            var ordensHoje = ordensHojeRequest.Data.Where(item => item.Side == BinanceNet.Enums.OrderSide.Sell).ToList();

            foreach (var ordem in ordens)
            {
                if (!ordensHoje.Any(item => item.OrderId.Equals(ordem.OrderSellId)))
                {
                    var ordemFaltante = _binanceClient.Spot.Order.GetOrder(ordem.Symbol, ordem.OrderSellId);

                    if (!ordemFaltante.Success)
                    {
                        _logger.LogError($"Erro conciliando ordens de venda abertas para moeda: {symbol} {JsonConvert.SerializeObject(ordensHojeRequest?.Error)}");
                        continue;
                    }

                    if (ordemFaltante.Data.Side == BinanceNet.Enums.OrderSide.Sell)
                    {
                        ordensHoje.Add(ordemFaltante.Data);
                    }
                }
            }

            foreach (var ordem in ordens)
            {
                var binanceOrder = ordensHoje.Where(item => item.Side == BinanceNet.Enums.OrderSide.Sell
                                                            && item.OrderId == ordem.OrderSellId).FirstOrDefault();

                if (binanceOrder == null) continue;

                if (binanceOrder.Status == BinanceNet.Enums.OrderStatus.Filled)
                {
                    ordem.Status = StatusOrdem.Vendida;
                    ordem.UltimaAtualizacao = DateTime.UtcNow;

                    await _monitorOrdemRepository.Update(ordem);

                    var msg = $"{DateTime.UtcNow} Ordem VENDA LIQUIDADA \n\nOrder Sell Id: {ordem.OrderSellId}\n\n Symbol:{ordem.Symbol} \n\n SPREAD: R$ {ordem.ValorSpread} - {ordem.PercentualSpread} % \n\n Tipo: {binanceOrder.Side} \n\nStatus: {binanceOrder.Status} \n\nPreço: {binanceOrder.Price} \n\nQuantidade: {binanceOrder.Quantity} \n\n Id Interno: {binanceOrder?.ClientOrderId}";
                    await _notificationService.SendMessage(EnvVariables.Telegram.Groups.BinancePrice, msg);
                }
                else if (binanceOrder.Status == BinanceNet.Enums.OrderStatus.Canceled || binanceOrder.Status == BinanceNet.Enums.OrderStatus.Expired)
                {
                    // ToDo - Definir como sera o rerpocessamento de ordens que ficaram presas
                    // Processar ordens canceladas que estavam pendentes

                    var msgNotificacao = $"{DateTime.UtcNow} PROCESSANDO ORDEM CANCELADA / EXPIRADA NA BINANCE \n\n STATUS MONITOR {ordem.Status.ToString()}  \n\n Status Binance: {binanceOrder.Status.ToString()} \n\n Ordem Id: {ordem._id} \n\n Ordem Compra: {ordem.OrderBuyId} \n\n Symbol: {ordem.Symbol}";
                    await _notificationService.SendMessage(EnvVariables.Telegram.Groups.BinancePrice, msgNotificacao);

                    var criptoConfig = await _configRepository.GetCriptoConfiguration(EnvVariables.USERNAME, symbol);

                    if (criptoConfig == null)
                    {
                        _logger.LogError($"Cripto config nao encontrado {symbol}");
                        return;
                    }

                    var saldoCriptoDisponivelCommand = await _commandDispatcher.DispatchAsync<BinanceBalanceFiatCommandInput>(new BinanceBalanceFiatCommandInput { Asset = criptoConfig.CriptoSymbol }) as BinanceBalanceFiatCommandOutput;

                    var saldoDisponivelCripto = saldoCriptoDisponivelCommand.Free;

                    var quantidadeVenda = Math.Round(binanceOrder.Quantity, criptoConfig.CasasDecimaisCripto);

                    if (saldoDisponivelCripto < quantidadeVenda)
                        quantidadeVenda = Math.Round(saldoDisponivelCripto, criptoConfig.CasasDecimaisCripto, MidpointRounding.ToZero);

                    var valorVenda = binanceOrder.Price;

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

                        msg = $"{DateTime.Now} Ordem VENDA ORIGINADA DE CANCELAMENTO COM ERRO - Retorno Binance: {(orderResult.Success ? "OK" : "NOK")}, Message: {JsonConvert.SerializeObject(orderResult?.Error)}, Json: {JsonConvert.SerializeObject(orderResult?.Data)} \n\n\n Order Internal Id: {ordem._id.ToString()} - \n\n Order Id: {ordem.OrderBuyId} \n\n  Moeda: {symbol} - Quantidade: {quantidadeVenda}, Preco: {valorVenda}, Id interno: {orderClientId}, saldo atual moeda: {saldoDisponivelCripto}";
                        await _notificationService.SendMessage(EnvVariables.Telegram.Groups.BinancePrice, msg);
                        continue;
                    }

                    ordem.OrderSellId = orderResult.Data.OrderId;
                    ordem.UltimaAtualizacao = DateTime.UtcNow;
                    ordem.Status = StatusOrdem.VendaCriada;
                    ordem.PrecoVenda = valorVenda;
                    ordem.ValorSpread = ordem.CalcularValorSpread();
                    ordem.PercentualSpread = ordem.CalcularPercentualSpread();

                    await _monitorOrdemRepository.Update(ordem);

                    msg = $"{DateTime.Now} Ordem VENDA ORIGINADA DE CANCELAMENTO ENVIADA - Retorno Binance: {(orderResult.Success ? "OK" : "NOK")},\n\nID ORDEM VENDA: {orderResult?.Data?.OrderId}\n\n ID ORDEM COMPRA: {ordem.OrderBuyId} \n\n Symbol:{orderResult?.Data?.Symbol} \n\nTipo: {orderResult?.Data?.Side.ToString()} \n\n Order Internal Id: {ordem._id.ToString()} \n\n Status: {orderResult?.Data?.Status.ToString()} \n\nPreço Ordem: {orderResult?.Data?.Price} \n\nPreço Medio: {ordem.PrecoCompra} \n\nQuantidade: {orderResult?.Data?.Quantity} \n\n Valor Spread: {ordem.ValorSpread} \n\n Percentual Spread: {ordem.PercentualSpread}  \n\n Id Referencia ordem binance: {orderResult?.Data?.ClientOrderId}";

                    await _notificationService.SendMessage(EnvVariables.Telegram.Groups.BinancePrice, msg);
                }
            }
        }
    }
}
