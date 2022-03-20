using CriptoTrade.Command.Binance.Trades;
using CriptoTrade.Command.Binance.ValidateKey;
using CriptoTrade.Command.Core.Ordem.Monitor.Compras;
using CriptoTrade.Command.Core.Ordem.Monitor.Vendas;
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

namespace CriptoTrade.Functions.TimerFunctions.Binance
{
    public class BinanceOrdemMonitorFunction
    {
        private readonly ICommandDispatcher _dispatcher;

        public BinanceOrdemMonitorFunction(ICommandDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [FunctionName("BinanceOrdemMonitorFunction")]
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


            log.LogInformation("Starting Ordem Monitor");

            var sw = Stopwatch.StartNew();

            var resultCompras = await _dispatcher.DispatchAsync<OrdemMonitorComprasCommandInput>(new OrdemMonitorComprasCommandInput { }) as OrdemMonitorComprasCommandOutput;
            var resultVendas = await _dispatcher.DispatchAsync<OrdemMonitorVendasCommandInput>(new OrdemMonitorVendasCommandInput { }) as OrdemMonitorComprasCommandOutput;

            sw.Stop();

            log.LogInformation($"Finish Ordem Monitor, Elapsed {sw.Elapsed}");
        }
    }
}
