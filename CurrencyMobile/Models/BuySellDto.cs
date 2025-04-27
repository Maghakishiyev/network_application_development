using System;

namespace CurrencyMobile.Models
{
    public class BuySellDto
    {
        public decimal Bid { get; set; }   // What bank buys at
        public decimal Ask { get; set; }   // What bank sells at
        public DateTime Date { get; set; } // publication date
    }
}