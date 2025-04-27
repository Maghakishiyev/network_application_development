using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CoreWCF;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using CurrencyData.Models;

namespace CurrencyService
{
    public class Service : IService
    {
        private readonly CurrencyData.Repositories.UserRepository _users;
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<Service> _log;

        public Service(
            CurrencyData.Repositories.UserRepository users,
            IHttpClientFactory httpFactory,
            ILogger<Service> log
        )
        {
            _users = users;
            _httpFactory = httpFactory;
            _log = log;
        }

        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }

        public async Task<decimal> GetCurrentRateAsync(string code)
        {
            // 1. Build the URL
            var url = $"https://api.nbp.pl/api/exchangerates/rates/a/{code}/?format=json";

            // 2. Fetch from NBP API
            var client = _httpFactory.CreateClient();
            var resp = await client.GetAsync(url);
            resp.EnsureSuccessStatusCode();

            // 3. Parse out the "mid" value
            using var stream = await resp.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            var mid = doc.RootElement
                        .GetProperty("rates")[0]
                        .GetProperty("mid")
                        .GetDecimal();

            _log.LogInformation("Fetched rate {Code} → {Mid}", code, mid);
            return mid;
        }
        
        public async Task<RateDto[]> GetHistoricalRatesAsync(
            string code, DateTime startDate, DateTime endDate)
        {
            // 1) Build NBP URL:
            var url = $"https://api.nbp.pl/api/exchangerates/rates/a/" +
                    $"{code}/{startDate:yyyy-MM-dd}/{endDate:yyyy-MM-dd}/?format=json";

            // 2) Fetch
            var client = _httpFactory.CreateClient();
            var resp = await client.GetAsync(url);
            resp.EnsureSuccessStatusCode();

            // 3) Parse JSON
            using var stream = await resp.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            var ratesElem = doc.RootElement.GetProperty("rates");

            var list = new List<RateDto>();
            foreach (var item in ratesElem.EnumerateArray())
            {
                list.Add(new RateDto
                {
                    Date = item.GetProperty("effectiveDate").GetDateTime(),
                    Mid = item.GetProperty("mid").GetDecimal()
                });
            }

            _log.LogInformation("Fetched historical rates for {Code}: {Count} data points", 
                code, list.Count);
            return list.ToArray();
        }
        
        public async Task<BuySellDto> GetBuySellRateAsync(string code)
        {
            var url = $"https://api.nbp.pl/api/exchangerates/rates/c/{code}/?format=json";
            var client = _httpFactory.CreateClient();
            var resp = await client.GetAsync(url);
            resp.EnsureSuccessStatusCode();

            using var stream = await resp.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            var root = doc.RootElement.GetProperty("rates")[0];

            var result = new BuySellDto
            {
                Date = root.GetProperty("effectiveDate").GetDateTime(),
                Bid = root.GetProperty("bid").GetDecimal(),
                Ask = root.GetProperty("ask").GetDecimal()
            };
            
            _log.LogInformation("Fetched buy/sell rate for {Code}: bid={Bid}, ask={Ask}", 
                code, result.Bid, result.Ask);
            return result;
        }
        
        public async Task<GoldDto> GetCurrentGoldPriceAsync()
        {
            var client = _httpFactory.CreateClient();
            var resp = await client.GetAsync("https://api.nbp.pl/api/cenyzlota/?format=json");
            resp.EnsureSuccessStatusCode();

            using var stream = await resp.Content.ReadAsStreamAsync();
            using var arr = await JsonDocument.ParseAsync(stream);
            var item = arr.RootElement[0];

            var result = new GoldDto
            {
                Date = item.GetProperty("data").GetDateTime(),
                Price = item.GetProperty("cena").GetDecimal()
            };
            
            _log.LogInformation("Fetched current gold price: {Price} PLN/g", result.Price);
            return result;
        }

        public async Task<GoldDto[]> GetHistoricalGoldPricesAsync(
            DateTime startDate, DateTime endDate)
        {
            var url = $"https://api.nbp.pl/api/cenyzlota/" +
                    $"{startDate:yyyy-MM-dd}/{endDate:yyyy-MM-dd}/?format=json";
            var client = _httpFactory.CreateClient();
            var resp = await client.GetAsync(url);
            resp.EnsureSuccessStatusCode();

            using var stream = await resp.Content.ReadAsStreamAsync();
            using var arr = await JsonDocument.ParseAsync(stream);

            var list = new List<GoldDto>();
            foreach (var item in arr.RootElement.EnumerateArray())
            {
                list.Add(new GoldDto
                {
                    Date = item.GetProperty("data").GetDateTime(),
                    Price = item.GetProperty("cena").GetDecimal()
                });
            }

            _log.LogInformation("Fetched historical gold prices: {Count} data points", list.Count);
            return list.ToArray();
        }
    }
}