
using CriptoTrade.Common.Env;
using CriptoTrade.Domain.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CriptoTrade.Command.Handlers
{
    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CommandDispatcher> _logger;

        public CommandDispatcher(IServiceProvider serviceProvider, ILogger<CommandDispatcher> logger)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task<ICommandOutput<TCommandInput>> DispatchAsync<TCommandInput>(TCommandInput input) where TCommandInput : ICommandInput
        {
            ICommandHandler<TCommandInput> handler;

            try
            {
                handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommandInput>>();
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Handler não localizado para o tipo: {input.GetType()}", ex);
            }

            try
            {
                return await handler.HandleAsync(input);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                string inputCommandString = null;

                try
                {
                    inputCommandString = JsonConvert.SerializeObject(input);
                }
                catch
                {
                }

                try
                {
                    var exMessage = ex.ToString();
                    var notificationService = _serviceProvider.GetRequiredService<INotification>();
                    await notificationService.SendMessage(EnvVariables.Telegram.Groups.BinancePrice, $"{DateTime.UtcNow} EXCEPTION NO COMMAND DISPATCHER -> {exMessage.Substring(0, Math.Min(2000, exMessage.Length))}");
                }
                catch
                {
                }

                _logger.LogError($"Erro executando o comando: {input.GetType()} com o handler: {handler.GetType()} --- Input: {inputCommandString} --- Exception: {ex.ToString()} ");
                throw ex;
            }
        }
    }
}
