using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace CriptoTrade.Domain.Model.BaseRepository
{
    public class BaseRepositoryEntity
    {
        public ObjectId _id { get; set; }
    }
}
