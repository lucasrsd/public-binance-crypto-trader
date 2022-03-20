using Binance.Net.Interfaces;
using CriptoTrade.Command.Handlers;
using CriptoTrade.Common.Env;
using CriptoTrade.Domain.Exceptions;
using CriptoTrade.Domain.Model.Binance.Trade;
using CriptoTrade.Domain.Repository.Core;
using CriptoTrade.Mapper.Binance;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CriptoTrade.Command.Binance.Ordens.Cancelar
{
    public class BinanceCancelarOrdemCommandHandler : ICommandHandler<BinanceCancelarOrdemCommandInput>
    {
        private readonly IBinanceClient _binanceClient;
        private readonly ILogger<BinanceCancelarOrdemCommandHandler> _logger;
        private readonly ICriptoOrdemMonitorRepository _monitorOrdemRepository;

        public BinanceCancelarOrdemCommandHandler(ILogger<BinanceCancelarOrdemCommandHandler> logger,
                                                    IBinanceClient binanceClient,
                                                    ICriptoOrdemMonitorRepository monitorOrdemRepository)
        {
            _binanceClient = binanceClient;
            _logger = logger;
            _monitorOrdemRepository = monitorOrdemRepository;
        }

        public async Task<ICommandOutput<BinanceCancelarOrdemCommandInput>> HandleAsync(BinanceCancelarOrdemCommandInput input)
        {
            if (input == null || string.IsNullOrEmpty(input.Symbol) || !input.OrderId.HasValue)
                throw new NullCommandInputException("Order Id / Symbol nao informado");

            _logger.LogInformation($"{input?.OrderId} Obtendo ordens abertas");

            var result = _binanceClient.Spot.Order.CancelOrder(input.Symbol, input.OrderId);

            if (result.Success)
            {
                var orderMonitorBuy = await _monitorOrdemRepository.GetByOrderBuyId(input.OrderId.Value);

                if (orderMonitorBuy != null)
                {
                    orderMonitorBuy.Status = Domain.Model.Core.Ordem.StatusOrdem.Cancelada;
                    orderMonitorBuy.UltimaAtualizacao = DateTime.UtcNow;

                    await _monitorOrdemRepository.Update(orderMonitorBuy);
                }

                _logger.LogInformation($"Ordem cancelada - {input.Symbol} - {input.OrderId}");
            }
            else
                throw new BinanceClientException($"Erro executando chamada na binance - {result.Error?.Code} {result.Error?.Message} {result.Error?.Data}");

            return new BinanceCancelarOrdemCommandOutput { };
        }
    }
}