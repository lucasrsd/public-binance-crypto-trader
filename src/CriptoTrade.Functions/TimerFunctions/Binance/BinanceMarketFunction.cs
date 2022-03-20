using CriptoTrade.Command.Binance.Trades;
using CriptoTrade.Command.Binance.ValidateKey;
using CriptoTrade.Command.Core.Trades.Buy;
using CriptoTrade.Command.Core.Trades.Targets;
using CriptoTrade.Command.Handlers;
using CriptoTrade.Common.Env;
using CriptoTrade.Domain.Interface;
using CriptoTrade.Domain.Model.Binance.PriceTicker;
using CriptoTrade.Domain.Repository.Binance;
using CriptoTrade.Domain.Repository.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static CriptoTrade.Common.Env.EnvVariables;

namespace CriptoTrade.Functions.TimerFunctions.Binance
{
    public class BinanceMarketFunction
    {
        private readonly ICommandDispatcher _dispatcher;
        private readonly ICriptoConfigRepository _criptoConfig;

        public BinanceMarketFunction(ICommandDispatcher dispatcher,
                                    ICriptoConfigRepository criptoConfig)
        {
            _dispatcher = dispatcher;
            _criptoConfig = criptoConfig;
        }

        [FunctionName("BinanceMarketFunction")]
        public async Task Run([TimerTrigger("0 */1 * * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            try
            {
                var result = await _dispatcher.DispatchAsync<BinanceValidateKeyCommandInput>(new BinanceValidateKeyCommandInput());
            }
            catch (Exception ex)
            {
                throw ex;
            }

            log.LogInformation($"Starting BinanceMarketFunction, user {EnvVariables.USERNAME}");

            var sw = Stopwatch.StartNew();

            var moedas = _criptoConfig.ObterMoedasProcessamentoAtivo(EnvVariables.USERNAME);
            var taskList = moedas.Select(symbol => Task.Run(() => RunSymbol(log, symbol)));
            Task.WaitAll(taskList.ToArray());

            sw.Stop();

            log.LogInformation($"Finish BinanceMarketFunction, Elapsed {sw.Elapsed}");
        }

        public async Task RunSymbol(ILogger log, string symbol)
        {
            var targets = await _dispatcher.DispatchAsync<CriptoTargetsCommandInput>(new CriptoTargetsCommandInput { Symbol = symbol }) as CriptoTargetsCommandOutput;

            var compras = await _dispatcher.DispatchAsync<CriptoBuyCommandInput>(new CriptoBuyCommandInput { Symbol = symbol, Targets = targets.Targets }) as CriptoBuyCommandOutput;
        }
    }
}
