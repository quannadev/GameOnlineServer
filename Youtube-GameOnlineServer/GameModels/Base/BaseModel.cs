using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Youtube_GameOnlineServer.GameModels.Base
{
    public class BaseModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        public BaseModel()
        {
            CreatedAt = DateTime.Now;
        }
    }
}