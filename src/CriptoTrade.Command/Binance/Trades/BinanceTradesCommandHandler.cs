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

namespace CriptoTrade.Command.Binance.Trades
{
    public class BinanceTradesCommandHandler : ICommandHandler<BinanceTradesCommandInput>
    {
        private readonly IBinanceClient _binanceClient;
        private readonly ILogger<BinanceTradesCommandHandler> _logger;
        private const int DEFAULT_TRADE_LIMIT = 1000;

        public BinanceTradesCommandHandler(ILogger<BinanceTradesCommandHandler> logger,
                                            IBinanceClient binanceClient)
        {
            _binanceClient = binanceClient;
            _logger = logger;
        }

        public async Task<ICommandOutput<BinanceTradesCommandInput>> HandleAsync(BinanceTradesCommandInput input)
        {
            if (input == null || string.IsNullOrEmpty(input?.Symbol))
                throw new NullCommandInputException("Simbolo / Request nao informado");

            _logger.LogInformation($"{input?.Symbol} Obtendo lista de trades recentes");

            var result = _binanceClient.Spot.Market.GetSymbolTrades(input?.Symbol, input?.Limit ?? DEFAULT_TRADE_LIMIT);

            if (!result.Success)
                throw new BinanceClientException($"Erro executando chamada na binance - {result.Error?.Code} {result.Error?.Message} {result.Error?.Data}");

            return new BinanceTradesCommandOutput
            {
                Trades = result.Data.Map().ToList()
            };
        }
    }
}