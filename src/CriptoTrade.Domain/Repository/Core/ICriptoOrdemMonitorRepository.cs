using CriptoTrade.Domain.Model.Binance.PriceTicker;
using CriptoTrade.Domain.Model.Core.Ordem;
using CriptoTrade.Domain.Model.Core.Parametros;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CriptoTrade.Domain.Repository.Core
{
    public interface ICriptoOrdemMonitorRepository
    {
        Task<MonitorOrdem> Insert(MonitorOrdem config);
        Task<MonitorOrdem> Update(MonitorOrdem ordem);
        Task<List<MonitorOrdem>> GetByStatus(string username, List<StatusOrdem> statusList);
        Task<List<MonitorOrdem>> ObterOrdensPendentesVenda(string username, string symbol);
        Task<MonitorOrdem> GetByOrderBuyId(long orderId);
        Task<MonitorOrdem> GetByOrderSellId(long orderId);
        Task CreateIndex();
    }
}
