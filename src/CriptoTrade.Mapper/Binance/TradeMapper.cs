using Binance.Net.Interfaces;
using CriptoTrade.Common.Extensions;
using CriptoTrade.Domain.Model.Binance.Trade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CriptoTrade.Mapper.Binance
{
    public static class TradeMapper
    {

        public static IEnumerable<TradeModel> Map(this IEnumerable<IBinanceRecentTrade> request)
        {
            if (request == null) return null;
            return request.Select(item => item.Map());
        }

        public static TradeModel Map(this IBinanceRecentTrade request)
        {
            if (request == null) return null;

            return new TradeModel
            {
                Id = request.OrderId,
                IsBestMatch = request.IsBestMatch,
                IsBuyerMaker = request.BuyerIsMaker,
                Price = request.Price,
                Quantity = request.BaseQuantity,
                QuoteQuantity = request.QuoteQuantity,
                Time = request.TradeTime
            };
        }
    }
}
