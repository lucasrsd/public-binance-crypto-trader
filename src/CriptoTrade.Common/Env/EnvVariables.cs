using System;
using System.Collections.Generic;
using System.Text;

namespace CriptoTrade.Common.Env
{
    public static class EnvVariables
    {
        public static string USERNAME = Environment.GetEnvironmentVariable("CRIPTOTRADE_USER");

        public class Database
        {
            public class MongoDb
            {
                public static string ConnectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTIONSTRING");
                public static string DatabaseName =  Environment.GetEnvironmentVariable("MONGODB_DBNAME");
            }
        }
        public class Binance
        { 
            public static string ApiKey => Environment.GetEnvironmentVariable("BINANCE_API_KEY");
            public static string ApiSecret => Environment.GetEnvironmentVariable("BINANCE_API_SECRET");
        }

        public class Telegram
        {
            public static string TelegramKey => Environment.GetEnvironmentVariable("TELEGRAM_KEY");

            public class Groups
            {
                
                public static string BinancePrice => Environment.GetEnvironmentVariable("TELEGRAM_BINANCE_GROUP");
            }
        }

        public class Trade
        {
            public readonly static decimal TAXA_TRADE = 0.001m;
            public readonly static decimal VALORIZACAO_PARA_VENDER = 1.004m;
            public readonly static decimal FAIXA_VALORIZACAO = 0.002m;
            public readonly static int TEMPO_SEGUNDOS_CANCELAR_ORDEM_COMPRA_NAO_EXECUTADA = -60;
        }
    }
}
