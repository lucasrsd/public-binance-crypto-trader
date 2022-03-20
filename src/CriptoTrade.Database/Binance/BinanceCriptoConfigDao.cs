using CriptoTrade.Domain.Model.Binance.PriceTicker;
using CriptoTrade.Domain.Model.Core.Parametros;
using CriptoTrade.Domain.Repository.Binance;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CriptoTrade.Database.Binance
{
    public class BinanceCriptoConfigDao : ICriptoConfigRepository
    {
        // ###### VERY IMPORTANT NOTE - I keep this class fixed for cost reasons, it should be moved to an S3 file or something like that, that has a low cost to read information that will not be updated with frequency.
        public BinanceCriptoConfigDao()
        {
        }

        public List<string> ObterMoedasProcessamentoAtivo(string user)
        {
            return new List<string> { "BTCBRL", "ETHBRL", "ADABRL", "DOTBRL", "ETCBRL", "LTCBRL", "XRPBRL", "MATICBRL", "LINKBRL", "ENJBRL", "CHZBRL", "SHIBBRL", "DOGEBRL" };
        }

        public async Task<CriptoConfig> GetCriptoConfiguration(string user, string symbol)
        {
            var configs = BuildConfig();
            return configs.Where(item => item.CriptoPair == symbol).FirstOrDefault();
        }

        private List<CriptoConfig> BuildConfig()
        {
            var cacheConfig = new List<CriptoConfig>();

            cacheConfig.Add(new CriptoConfig { CriptoPair = "BTCBRL", CriptoSymbol = "BTC", CasasDecimaisCripto = 5, CasasDecimaisFiat = 0, ValorAporte = 150, LimiteFiatCripto = 12000 });
            cacheConfig.Add(new CriptoConfig { CriptoPair = "ETHBRL", CriptoSymbol = "ETH", CasasDecimaisCripto = 4, CasasDecimaisFiat = 2, ValorAporte = 100, LimiteFiatCripto = 10000 });
            cacheConfig.Add(new CriptoConfig { CriptoPair = "ADABRL", CriptoSymbol = "ADA", CasasDecimaisCripto = 1, CasasDecimaisFiat = 3, ValorAporte = 70, LimiteFiatCripto = 8500 });
            cacheConfig.Add(new CriptoConfig { CriptoPair = "DOTBRL", CriptoSymbol = "DOT", CasasDecimaisCripto = 2, CasasDecimaisFiat = 2, ValorAporte = 25, LimiteFiatCripto = 2500 });
            cacheConfig.Add(new CriptoConfig { CriptoPair = "ETCBRL", CriptoSymbol = "ETC", CasasDecimaisCripto = 2, CasasDecimaisFiat = 1, ValorAporte = 25, LimiteFiatCripto = 1500 });
            cacheConfig.Add(new CriptoConfig { CriptoPair = "LTCBRL", CriptoSymbol = "LTC", CasasDecimaisCripto = 3, CasasDecimaisFiat = 1, ValorAporte = 25, LimiteFiatCripto = 2000 });
            cacheConfig.Add(new CriptoConfig { CriptoPair = "XRPBRL", CriptoSymbol = "XRP", CasasDecimaisCripto = 0, CasasDecimaisFiat = 3, ValorAporte = 25, LimiteFiatCripto = 2000 });

            cacheConfig.Add(new CriptoConfig { CriptoPair = "MATICBRL", CriptoSymbol = "MATIC", CasasDecimaisCripto = 1, CasasDecimaisFiat = 3, ValorAporte = 25, LimiteFiatCripto = 1000 });
            cacheConfig.Add(new CriptoConfig { CriptoPair = "LINKBRL", CriptoSymbol = "LINK", CasasDecimaisCripto = 2, CasasDecimaisFiat = 1, ValorAporte = 25, LimiteFiatCripto = 1000 });
            cacheConfig.Add(new CriptoConfig { CriptoPair = "HOTBRL", CriptoSymbol = "HOT", CasasDecimaisCripto = 0, CasasDecimaisFiat = 4, ValorAporte = 25, LimiteFiatCripto = 1000 });
            cacheConfig.Add(new CriptoConfig { CriptoPair = "ENJBRL", CriptoSymbol = "ENJ", CasasDecimaisCripto = 1, CasasDecimaisFiat = 3, ValorAporte = 25, LimiteFiatCripto = 1000 });
            cacheConfig.Add(new CriptoConfig { CriptoPair = "CHZBRL", CriptoSymbol = "CHZ", CasasDecimaisCripto = 0, CasasDecimaisFiat = 3, ValorAporte = 25, LimiteFiatCripto = 1000 });
            cacheConfig.Add(new CriptoConfig { CriptoPair = "SHIBBRL", CriptoSymbol = "SHIB", CasasDecimaisCripto = 0, CasasDecimaisFiat = 8, ValorAporte = 25, LimiteFiatCripto = 1000 });
            cacheConfig.Add(new CriptoConfig { CriptoPair = "DOGEBRL", CriptoSymbol = "DOGE", CasasDecimaisCripto = 0, CasasDecimaisFiat = 3, ValorAporte = 25, LimiteFiatCripto = 2000 });

            return cacheConfig;
        }
    }
}
