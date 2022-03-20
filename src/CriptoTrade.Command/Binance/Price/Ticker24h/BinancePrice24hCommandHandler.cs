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

namespace CriptoTrade.Command.Binance.Price.Ticker24h
{
    public class BinancePrice24hCommandHandler : ICommandHandler<BinancePrice24hCommandInput>
    {
        private readonly IBinanceClient _binanceClient;
        private readonly ILogger<BinancePrice24hCommandHandler> _logger;

        public BinancePrice24hCommandHandler(ILogger<BinancePrice24hCommandHandler> logger,
                                            IBinanceClient binanceClient)
        {
            _binanceClient = binanceClient;
            _logger = logger;
        }

        public async Task<ICommandOutput<BinancePrice24hCommandInput>> HandleAsync(BinancePrice24hCommandInput input)
        {
            if (input == null || string.IsNullOrEmpty(input?.Symbol))
                throw new NullCommandInputException("Simbolo / Request nao informado");

            _logger.LogInformation($"{input?.Symbol} Obtendo lista de trades recentes");

            var result = _binanceClient.Spot.Market.Get24HPrice(input.Symbol);

            if (!result.Success)
                throw new BinanceClientException($"Erro executando chamada na binance - {result.Error?.Code} {result.Error?.Message} {result.Error?.Data}");

            return new BinancePrice24hCommandOutput
            {
                Price = result.Data
            };
        }
    }
}