using System.Collections.Generic;

namespace CurrencyMobile.Models
{
    public class AccountDto
    {
        public string UserId { get; set; } = null!;
        public Dictionary<string, decimal> Balances { get; set; } = new();
    }
}