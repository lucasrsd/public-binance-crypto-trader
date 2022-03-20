using CriptoTrade.Common.Env;
using CriptoTrade.Domain.Interface;
using System;
using System.Threading.Tasks;
using Telegram.Bot;

namespace CriptoTrade.Notification
{
    public class TelegramMessage : INotification
    {
        private readonly string _telegramKey;
        public TelegramMessage(string telegramKey)
        {
            _telegramKey = telegramKey;
        }
        public async Task SendMessage(string group, string message)
        {
            try
            {
                var key = EnvVariables.Binance.ApiKey ?? "";
                message = "(API KEY: " + key.Substring(0, Math.Min(key.Length, 6)) + "...) \n\n" + message;
                var bot = new TelegramBotClient(_telegramKey);

                await bot.SendTextMessageAsync(group, message);
            }
            catch { }
        }
    }
}
