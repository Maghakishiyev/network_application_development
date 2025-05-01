using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace CurrencyData
{
    public class User
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}