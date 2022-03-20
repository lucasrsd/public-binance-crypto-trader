using Binance.Net.Interfaces;
using CriptoTrade.Command.Handlers;
using CriptoTrade.Domain.Exceptions;
using CriptoTrade.Domain.Model.Binance.Trade;
using CriptoTrade.Mapper.Binance;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CriptoTrade.Command.Binance.Ordens.List
{
    public class BinanceOrdensCommandHandler : ICommandHandler<BinanceOrdensCommandInput>
    {
        private readonly IBinanceClient _binanceClient;
        private readonly ILogger<BinanceOrdensCommandHandler> _logger;

        public BinanceOrdensCommandHandler(ILogger<BinanceOrdensCommandHandler> logger,
                                            IBinanceClient binanceClient)
        {
            _binanceClient = binanceClient;
            _logger = logger;
        }

        public async Task<ICommandOutput<BinanceOrdensCommandInput>> HandleAsync(BinanceOrdensCommandInput input)
        {
            if (input == null || string.IsNullOrEmpty(input?.Symbol))
                throw new NullCommandInputException("Simbolo / Request nao informado");

            _logger.LogInformation($"{input?.Symbol} Obtendo ordens abertas");

            var ordens = _binanceClient.Spot.Order.GetOpenOrders(input.Symbol);

            if (!ordens.Success)
                throw new BinanceClientException($"Erro executando chamada na binance - {ordens.Error?.Code} {ordens.Error?.Message} {ordens.Error?.Data}");


            return new BinanceOrdensCommandOutput { OrdensAbertas = ordens.Data.ToList() };
        }
    }
}