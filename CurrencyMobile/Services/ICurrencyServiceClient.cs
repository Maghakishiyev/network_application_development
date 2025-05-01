using System;
using System.Threading.Tasks;
using CurrencyMobile.Models;

namespace CurrencyMobile.Services
{
    public interface ICurrencyServiceClient
    {
        Task<UserDto> AuthenticateAsync(string email, string password);
        Task<UserDto> RegisterUserAsync(string email, string password);
        Task<decimal> GetCurrentRateAsync(string code);
        Task<RateDto[]> GetHistoricalRatesAsync(string code, DateTime start, DateTime end);
        Task<BuySellDto> GetBuySellRateAsync(string code);
        Task<GoldDto> GetCurrentGoldPriceAsync();
        Task<GoldDto[]> GetHistoricalGoldPricesAsync(DateTime start, DateTime end);
        Task<AccountDto> GetAccountAsync(string userId);
        Task<TradeResultDto> BuyCurrencyAsync(string userId, string code, decimal pln);
        Task<TradeResultDto> SellCurrencyAsync(string userId, string code, decimal foreign);
        Task<Transaction[]> GetTransactionsAsync(string userId);
    }
}