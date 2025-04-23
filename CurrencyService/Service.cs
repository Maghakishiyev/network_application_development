using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CoreWCF;
using Microsoft.Extensions.Logging;

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
    }
}