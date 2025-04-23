using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace CurrencyData
{
    public class User
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
    }
}