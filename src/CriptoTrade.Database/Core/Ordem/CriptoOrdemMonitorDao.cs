using CriptoTrade.Domain.Model.Core.Ordem;
using CriptoTrade.Domain.Repository.Core;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CriptoTrade.Database.Core.Ordem
{
    public class CriptoOrdemMonitorDao : ICriptoOrdemMonitorRepository
    {
        public CriptoOrdemMonitorDao(IMongoCollection<MonitorOrdem> collection)
        {
            _collection = collection;
        }

        private readonly IMongoCollection<MonitorOrdem> _collection;

        public async Task<MonitorOrdem> Insert(MonitorOrdem ordem)
        {
            await _collection.InsertOneAsync(ordem);
            return ordem;
        }

        public async Task<MonitorOrdem> Update(MonitorOrdem ordem)
        {
            await _collection.ReplaceOneAsync(item => item._id == ordem._id, ordem);
            return ordem;
        }

        public async Task<List<MonitorOrdem>> GetByStatus(string username, List<StatusOrdem> statusList)
        {
            var filter = Builders<MonitorOrdem>.Filter.In(item => item.Status, statusList);

            var findResult = await _collection.FindAsync(filter);
            return findResult.ToList().Where(item => item.Username != null && item.Username.Equals(username, System.StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        public async Task<List<MonitorOrdem>> ObterOrdensPendentesVenda(string username, string symbol)
        {
            var result = await GetByStatus(username, new List<StatusOrdem> { StatusOrdem.Pendente, StatusOrdem.CompraConfirmada, StatusOrdem.VendaCriada });
            if (result != null && result.Any() && result.Any(item => item != null))
                return result.Where(item => item.Symbol == symbol).ToList();

            return null;
        }

        public async Task<MonitorOrdem> GetByOrderBuyId(long orderId)
        {
            var filter = Builders<MonitorOrdem>.Filter.Eq(x => x.OrderBuyId, orderId);

            var findResult = await _collection.FindAsync(filter);
            return findResult.FirstOrDefault();
        }

        public async Task<MonitorOrdem> GetByOrderSellId(long orderId)
        {
            var filter = Builders<MonitorOrdem>.Filter.Eq(x => x.OrderSellId, orderId);

            var findResult = await _collection.FindAsync(filter);
            return findResult.FirstOrDefault();
        }

        public async Task CreateIndex()
        {
            var indexDate = Builders<MonitorOrdem>.IndexKeys.Descending(config => config.CreationDate);
            var indexOrderBuyId = Builders<MonitorOrdem>.IndexKeys.Descending(config => config.OrderBuyId);
            var indexOrderSell = Builders<MonitorOrdem>.IndexKeys.Descending(config => config.OrderSellId);
            var indexStatus = Builders<MonitorOrdem>.IndexKeys.Descending(config => config.Status);
            var indexSymbol = Builders<MonitorOrdem>.IndexKeys.Descending(config => config.Symbol);
            var indexUser = Builders<MonitorOrdem>.IndexKeys.Descending(config => config.Username);

            await _collection.Indexes.CreateOneAsync(new CreateIndexModel<MonitorOrdem>(indexDate));
            await _collection.Indexes.CreateOneAsync(new CreateIndexModel<MonitorOrdem>(indexOrderBuyId));
            await _collection.Indexes.CreateOneAsync(new CreateIndexModel<MonitorOrdem>(indexOrderSell));
            await _collection.Indexes.CreateOneAsync(new CreateIndexModel<MonitorOrdem>(indexStatus));
            await _collection.Indexes.CreateOneAsync(new CreateIndexModel<MonitorOrdem>(indexSymbol));
            await _collection.Indexes.CreateOneAsync(new CreateIndexModel<MonitorOrdem>(indexUser));
        }
    }
}
