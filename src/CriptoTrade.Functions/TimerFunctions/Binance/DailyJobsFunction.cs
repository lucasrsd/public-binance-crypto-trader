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
    public class DailyJobsFunction
    {
        private readonly ICommandDispatcher _dispatcher;
        private readonly ICriptoOrdemMonitorRepository _monitorDb;

        public DailyJobsFunction(ICommandDispatcher dispatcher,
                                ICriptoOrdemMonitorRepository monitorDb)
        {
            _dispatcher = dispatcher;
            _monitorDb = monitorDb;
        }

        [FunctionName("DailyJobsFunction")]
        public async Task Run([TimerTrigger("0 30 9 * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation("Starting Daily jobs");

            log.LogInformation("Iniciando processamento de indices");
            await _monitorDb.CreateIndex();
            log.LogInformation($"Finalziado - Proximo schedule  {myTimer?.ScheduleStatus?.Next}");

            log.LogInformation($"Finish Daily jobs");
        }
    }
}
