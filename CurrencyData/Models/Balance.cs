using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace CurrencyData
{
    public class Balance
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ObjectId UserId { get; set; }
        public string CurrencyCode { get; set; } = null!;
        public decimal Amount { get; set; }
    }
}