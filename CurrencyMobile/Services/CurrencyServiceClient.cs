using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using CurrencyMobile.Models;

namespace CurrencyMobile.Services
{
    public class CurrencyServiceClient : ICurrencyServiceClient
    {
        private readonly string _serviceUrl;
        private readonly BasicHttpBinding _binding;

        public CurrencyServiceClient()
        {
            string serviceUrl;

            // When running on Android emulator, use the special IP 10.0.2.2 which points to the host machine
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                serviceUrl = "http://10.0.2.2:5001/";
                Console.WriteLine($"Running on Android, using service URL: {serviceUrl}");
            }
            // For iOS simulator
            else if (DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.DeviceType == DeviceType.Virtual)
            {
                serviceUrl = "http://localhost:5001/";
                Console.WriteLine($"Running on iOS simulator, using service URL: {serviceUrl}");
            }
            // For everything else (including desktop)
            else
            {
                serviceUrl = "http://localhost:5001/";
                Console.WriteLine($"Running on {DeviceInfo.Platform}, using service URL: {serviceUrl}");
            }

            _serviceUrl = serviceUrl;

            _binding = new BasicHttpBinding(BasicHttpSecurityMode.None)
            {
                MaxReceivedMessageSize = 10_000_000,
                OpenTimeout = TimeSpan.FromSeconds(30),
                SendTimeout = TimeSpan.FromSeconds(30),
                ReceiveTimeout = TimeSpan.FromSeconds(60),
                AllowCookies = true,
                MaxBufferPoolSize = 10_000_000,
                MaxBufferSize = 10_000_000,
                TextEncoding = System.Text.Encoding.UTF8,
                UseDefaultWebProxy = true
            };
        }
        
        public async Task<UserDto> AuthenticateAsync(string email, string password)
        {
            try
            {
                var channel = CreateChannel<CurrencyService.IService>();
                var user = await channel.AuthenticateAsync(email, password);
                
                return new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication error: {ex.Message}");
                throw;
            }
        }
        
        public async Task<UserDto> RegisterUserAsync(string email, string password)
        {
            try
            {
                var channel = CreateChannel<CurrencyService.IService>();
                var user = await channel.RegisterUserAsync(email, password);
                
                return new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                throw;
            }
        }

        private T CreateChannel<T>() where T : class
        {
            var factory = new ChannelFactory<T>(_binding, new EndpointAddress(_serviceUrl));
            return factory.CreateChannel();
        }

        public async Task<decimal> GetCurrentRateAsync(string code)
        {
            var channel = CreateChannel<CurrencyService.IService>();
            return await channel.GetCurrentRateAsync(code);
        }

        public async Task<RateDto[]> GetHistoricalRatesAsync(string code, DateTime start, DateTime end)
        {
            var channel = CreateChannel<CurrencyService.IService>();
            var rates = await channel.GetHistoricalRatesAsync(code, start, end);
            return rates.Select(r => new RateDto { Date = r.Date, Mid = r.Mid }).ToArray();
        }

        public async Task<BuySellDto> GetBuySellRateAsync(string code)
        {
            var channel = CreateChannel<CurrencyService.IService>();
            var rate = await channel.GetBuySellRateAsync(code);
            return new BuySellDto { Bid = rate.Bid, Ask = rate.Ask, Date = rate.Date };
        }

        public async Task<GoldDto> GetCurrentGoldPriceAsync()
        {
            var channel = CreateChannel<CurrencyService.IService>();
            var gold = await channel.GetCurrentGoldPriceAsync();
            return new GoldDto { Date = gold.Date, Price = gold.Price };
        }

        public async Task<GoldDto[]> GetHistoricalGoldPricesAsync(DateTime start, DateTime end)
        {
            var channel = CreateChannel<CurrencyService.IService>();
            var prices = await channel.GetHistoricalGoldPricesAsync(start, end);
            return prices.Select(p => new GoldDto { Date = p.Date, Price = p.Price }).ToArray();
        }

        public async Task<AccountDto> GetAccountAsync(string userId)
        {
            var channel = CreateChannel<CurrencyService.IService>();
            var account = await channel.GetAccountAsync(userId);
            return new AccountDto { UserId = account.UserId, Balances = new Dictionary<string, decimal>(account.Balances) };
        }

        public async Task<TradeResultDto> BuyCurrencyAsync(string userId, string code, decimal pln)
        {
            var channel = CreateChannel<CurrencyService.IService>();
            var result = await channel.BuyCurrencyAsync(userId, code, pln);
            return new TradeResultDto
            {
                UserId = result.UserId,
                CurrencyCode = result.CurrencyCode,
                AmountForeign = result.AmountForeign,
                AmountPln = result.AmountPln,
                Rate = result.Rate,
                Timestamp = result.Timestamp
            };
        }

        public async Task<TradeResultDto> SellCurrencyAsync(string userId, string code, decimal foreign)
        {
            var channel = CreateChannel<CurrencyService.IService>();
            var result = await channel.SellCurrencyAsync(userId, code, foreign);
            return new TradeResultDto
            {
                UserId = result.UserId,
                CurrencyCode = result.CurrencyCode,
                AmountForeign = result.AmountForeign,
                AmountPln = result.AmountPln,
                Rate = result.Rate,
                Timestamp = result.Timestamp
            };
        }

        public async Task<Transaction[]> GetTransactionsAsync(string userId)
        {
            var channel = CreateChannel<CurrencyService.IService>();
            var transactions = await channel.GetTransactionsAsync(userId);
            return transactions.Select(t => new Transaction
            {
                Id = t.Id.ToString(),
                UserId = t.UserId.ToString(),
                Type = t.Type,
                CurrencyCode = t.CurrencyCode,
                Amount = t.Amount,
                Timestamp = t.Timestamp
            }).ToArray();
        }
    }
}