using System;

namespace CurrencyData.Models
{
    public class TradeResultDto
    {
        public string UserId { get; set; } = null!;
        public string CurrencyCode { get; set; } = null!;
        public decimal AmountForeign { get; set; }
        public decimal AmountPln { get; set; }
        public decimal Rate { get; set; }
        public DateTime Timestamp { get; set; }
    }
}