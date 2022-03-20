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

namespace CriptoTrade.Command.Binance.Balance.Fiat
{
    public class BinanceBalanceFiatCommandHandler : ICommandHandler<BinanceBalanceFiatCommandInput>
    {
        private readonly IBinanceClient _binanceClient;
        private readonly ILogger<BinanceBalanceFiatCommandHandler> _logger;

        public BinanceBalanceFiatCommandHandler(ILogger<BinanceBalanceFiatCommandHandler> logger,
                                            IBinanceClient binanceClient)
        {
            _binanceClient = binanceClient;
            _logger = logger;
        }

        public async Task<ICommandOutput<BinanceBalanceFiatCommandInput>> HandleAsync(BinanceBalanceFiatCommandInput input)
        {
            if (input == null || string.IsNullOrEmpty(input.Asset))
                throw new NullCommandInputException("Order Id / Asset nao informado");

            _logger.LogInformation($"{input?.Asset} Obtendo saldo");

            var accountInfo = _binanceClient.General.GetAccountInfo();

            if (!accountInfo.Success)
                throw new Exception($"Erro consultando saldo da moeda {input.Asset}, " + accountInfo?.Error?.Message);

            var saldo = accountInfo.Data?.Balances?.Where(item => item.Asset.Equals(input.Asset)).FirstOrDefault();

            if (saldo == null)
                throw new Exception($"Saldo {input.Asset} nao encontrado no balanco via get account info");

            return new BinanceBalanceFiatCommandOutput
            {
                Asset = saldo.Asset,
                Free = saldo.Free,
                Locked = saldo.Locked,
                Total = saldo.Total
            };
        }
    }
}