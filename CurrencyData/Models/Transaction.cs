using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
namespace CurrencyData
{
    public class Transaction
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ObjectId UserId { get; set; }
        public string Type { get; set; } = null!; // "buy" or "sell"
        public string CurrencyCode { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}