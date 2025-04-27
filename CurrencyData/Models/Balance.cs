using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace CurrencyData
{
    public class Balance
    {
        [BsonId]
        public ObjectId Id { get; set; }
        
        public ObjectId UserId { get; set; }
        
        // Dictionary of currency codes to amounts
        public Dictionary<string, decimal> Currencies { get; set; } = new Dictionary<string, decimal>();
        
        // Helper method to get a currency balance (returns 0 if not found)
        public decimal GetAmount(string currencyCode)
        {
            return Currencies.TryGetValue(currencyCode, out decimal amount) ? amount : 0m;
        }
        
        // Helper method to set a currency balance
        public void SetAmount(string currencyCode, decimal amount)
        {
            Currencies[currencyCode] = amount;
        }
        
        // Helper method to adjust a currency balance by the given delta
        public void AdjustAmount(string currencyCode, decimal delta)
        {
            var current = GetAmount(currencyCode);
            SetAmount(currencyCode, current + delta);
        }
    }
}