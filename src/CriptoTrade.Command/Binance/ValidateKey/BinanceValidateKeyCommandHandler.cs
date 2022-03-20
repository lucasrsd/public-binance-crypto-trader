using Binance.Net.Interfaces;
using CriptoTrade.Command.Handlers;
using CriptoTrade.Common.Env;
using CriptoTrade.Domain.Exceptions;
using CriptoTrade.Mapper.Binance;
using CryptoExchange.Net.RateLimiter;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace CriptoTrade.Command.Binance.ValidateKey
{
    public class BinanceValidateKeyCommandHandler : ICommandHandler<BinanceValidateKeyCommandInput>
    {
        private readonly IBinanceClient _binanceClient;
        private readonly ILogger<BinanceValidateKeyCommandHandler> _logger;

        public BinanceValidateKeyCommandHandler(ILogger<BinanceValidateKeyCommandHandler> logger,
                                            IBinanceClient binanceClient)
        {
            _binanceClient = binanceClient;
            _logger = logger;
        }

        public async Task<ICommandOutput<BinanceValidateKeyCommandInput>> HandleAsync(BinanceValidateKeyCommandInput input)
        {
            if (string.IsNullOrEmpty( EnvVariables.USERNAME))
            {
                throw new BinanceAPIInvalid("Username nao cadastrado no env variables");
            }

            var tradingStatus = _binanceClient.General.GetTradingStatus();

            if (!tradingStatus.Success)
                throw new BinanceAPIInvalid($"{JsonConvert.SerializeObject(tradingStatus?.Error)}");

            if (tradingStatus.Data.IsLocked)
                throw new BinanceAPIInvalid($"Account locked, {JsonConvert.SerializeObject(tradingStatus?.Data)}");

            return new BinanceValidateKeyCommandOutput
            {
                Success = true
            };
        }
    }
}