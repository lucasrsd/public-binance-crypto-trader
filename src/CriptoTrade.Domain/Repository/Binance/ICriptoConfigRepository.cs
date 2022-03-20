using CriptoTrade.Domain.Model.Core.Parametros;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CriptoTrade.Domain.Repository.Binance
{
    public interface ICriptoConfigRepository
    {
        List<string> ObterMoedasProcessamentoAtivo(string user);
        Task<CriptoConfig> GetCriptoConfiguration(string username, string symbol);
    }
}
