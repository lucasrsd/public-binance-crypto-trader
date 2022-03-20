using Binance.Net;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Spot;
using CriptoTrade.Command.Binance.Ordens.Cancelar;
using CriptoTrade.Command.Binance.Ordens.List;
using CriptoTrade.Command.Binance.Price.Ticker24h;
using CriptoTrade.Command.Binance.Trades;
using CriptoTrade.Command.Core.Trades.Buy;
using CriptoTrade.Command.Core.Ordem.Monitor.Compras;
using CriptoTrade.Command.Core.Ordem.Monitor.Vendas;
using CriptoTrade.Command.Core.Trades.Targets;
using CriptoTrade.Command.Handlers;
using CriptoTrade.Common.Env;
using CriptoTrade.Database.Binance;
using CriptoTrade.Domain.Interface;
using CriptoTrade.Command.Binance.Balance.Fiat;
using CriptoTrade.Domain.Model.Core.Parametros;
using CriptoTrade.Domain.Repository.Binance;
using CriptoTrade.Notification;
using CryptoExchange.Net.Authentication;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using System.Security.Authentication;
using CriptoTrade.Domain.Model.Core.Ordem;
using CriptoTrade.Domain.Repository.Core;
using CriptoTrade.Database.Core.Ordem;
using Microsoft.Extensions.Logging;
using CriptoTrade.Command.Binance.ValidateKey;


[assembly: FunctionsStartup(typeof(CriptoTrade.Functions.Startup))]

namespace CriptoTrade.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.SetMinimumLevel(LogLevel.Debug);
            });

            // Notifications clientes

            builder.Services.AddTransient<INotification>((s) =>
            {
                return new TelegramMessage(EnvVariables.Telegram.TelegramKey);
            });

            // Exchanges - Binance

            builder.Services.AddTransient<IBinanceClient>((s) =>
            {
                return new BinanceClient(new BinanceClientOptions
                {
                    ApiCredentials = new ApiCredentials(EnvVariables.Binance.ApiKey, EnvVariables.Binance.ApiSecret),
                    RateLimitingBehaviour = CryptoExchange.Net.Objects.RateLimitingBehaviour.Fail,
                    LogVerbosity = CryptoExchange.Net.Logging.LogVerbosity.Info,
                    RequestTimeout = TimeSpan.FromSeconds(10),
                    AutoTimestamp = true
                });
            });

            // Handlers

            builder.Services.AddTransient<ICommandDispatcher, CommandDispatcher>();
            builder.Services.AddTransient<ICommandHandler<BinanceTradesCommandInput>, BinanceTradesCommandHandler>();
            builder.Services.AddTransient<ICommandHandler<CriptoTargetsCommandInput>, CriptoTargetsCommandHandler>();
            builder.Services.AddTransient<ICommandHandler<BinanceOrdensCommandInput>, BinanceOrdensCommandHandler>();
            builder.Services.AddTransient<ICommandHandler<BinanceCancelarOrdemCommandInput>, BinanceCancelarOrdemCommandHandler>();
            builder.Services.AddTransient<ICommandHandler<BinancePrice24hCommandInput>, BinancePrice24hCommandHandler>();
            builder.Services.AddTransient<ICommandHandler<CriptoBuyCommandInput>, CriptoBuyCommandHandler>();
            builder.Services.AddTransient<ICommandHandler<BinanceBalanceFiatCommandInput>, BinanceBalanceFiatCommandHandler>();
            builder.Services.AddTransient<ICommandHandler<OrdemMonitorComprasCommandInput>, OrdemMonitorComprasCommandHandler>();
            builder.Services.AddTransient<ICommandHandler<OrdemMonitorVendasCommandInput>, OrdemMonitorVendasCommandHandler>();
            builder.Services.AddTransient<ICommandHandler<BinanceValidateKeyCommandInput>, BinanceValidateKeyCommandHandler>();

            // Databases

            builder.Services.AddTransient<IMongoDatabase>((item) =>
            {
                MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(EnvVariables.Database.MongoDb.ConnectionString));

                settings.SslSettings =
                          new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
                var client = new MongoClient(settings);

                return client.GetDatabase(EnvVariables.Database.MongoDb.DatabaseName);
            });

            builder.Services.AddTransient<IMongoCollection<MonitorOrdem>>((s) =>
            {
                var db = s.GetRequiredService<IMongoDatabase>();
                return db.GetCollection<MonitorOrdem>("BinanceMonitorOrdem");
            });

            // DAOs

            builder.Services.AddTransient<ICriptoConfigRepository, BinanceCriptoConfigDao>();
            builder.Services.AddTransient<ICriptoOrdemMonitorRepository, CriptoOrdemMonitorDao>();
        }
    }
}
