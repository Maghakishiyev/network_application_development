using System;

namespace CurrencyMobile.Models
{
    public class Transaction
    {
        public string Id { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string Type { get; set; } = null!; // "buy" or "sell"
        public string CurrencyCode { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}