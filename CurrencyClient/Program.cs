using System;
using System.ServiceModel;
using System.Threading.Tasks;
using CurrencyClient;
using CurrencyData.Models;

[ServiceContract(Name = "IService", Namespace = "http://currency.service")]
public interface ICurrencyService
{
    [OperationContract]
    Task<decimal> GetCurrentRateAsync(string code);
    
    [OperationContract]
    Task<RateDto[]> GetHistoricalRatesAsync(string code, DateTime startDate, DateTime endDate);
    
    [OperationContract]
    Task<BuySellDto> GetBuySellRateAsync(string code);
    
    [OperationContract]
    Task<GoldDto> GetCurrentGoldPriceAsync();
    
    [OperationContract]
    Task<GoldDto[]> GetHistoricalGoldPricesAsync(DateTime startDate, DateTime endDate);
}

public class CurrencyServiceClient : ClientBase<ICurrencyService>, ICurrencyService
{
    public CurrencyServiceClient(System.ServiceModel.Channels.Binding binding, EndpointAddress address) 
        : base(binding, address) { }
    
    public Task<decimal> GetCurrentRateAsync(string code)
    {
        return Channel.GetCurrentRateAsync(code);
    }
    
    public Task<RateDto[]> GetHistoricalRatesAsync(string code, DateTime startDate, DateTime endDate)
    {
        return Channel.GetHistoricalRatesAsync(code, startDate, endDate);
    }
    
    public Task<BuySellDto> GetBuySellRateAsync(string code)
    {
        return Channel.GetBuySellRateAsync(code);
    }
    
    public Task<GoldDto> GetCurrentGoldPriceAsync()
    {
        return Channel.GetCurrentGoldPriceAsync();
    }
    
    public Task<GoldDto[]> GetHistoricalGoldPricesAsync(DateTime startDate, DateTime endDate)
    {
        return Channel.GetHistoricalGoldPricesAsync(startDate, endDate);
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Currency Exchange WCF Client");
        Console.WriteLine("============================");
        
        await WcfTestClient.TestWcfService();
        
        try
        {
            Console.WriteLine("Connecting to WCF service at http://localhost:5000/");
            
            var binding = new BasicHttpBinding { 
                Security = { Mode = BasicHttpSecurityMode.None },
                MaxReceivedMessageSize = 10 * 1024 * 1024, // 10MB
                OpenTimeout = TimeSpan.FromSeconds(5),
                SendTimeout = TimeSpan.FromSeconds(30),
                ReceiveTimeout = TimeSpan.FromSeconds(30)
            };
            var address = new EndpointAddress("http://localhost:5000/");
            
            Console.WriteLine("Creating WCF client channel...");
            using (var client = new CurrencyServiceClient(binding, address))
            {
                try
                {
                    ((ICommunicationObject)client).Open();
                    Console.WriteLine("Channel opened successfully");
                    
                    // Test current rate
                    Console.WriteLine("\n=== CURRENT RATE TEST ===");
                    await GetCurrentRate(client, "USD");
                    
                    // Test historical rates
                    Console.WriteLine("\n=== HISTORICAL RATES TEST ===");
                    await TestHistoricalRates(client, "USD", 
                        DateTime.Now.AddDays(-10), DateTime.Now);
                    
                    // Test buy/sell rates
                    Console.WriteLine("\n=== BUY/SELL RATE TEST ===");
                    await TestBuySellRate(client, "EUR");
                    
                    // Test gold price
                    Console.WriteLine("\n=== GOLD PRICE TEST ===");
                    await TestGoldPrice(client);
                    
                    // Test historical gold prices
                    Console.WriteLine("\n=== HISTORICAL GOLD PRICES TEST ===");
                    await TestHistoricalGoldPrices(client, 
                        DateTime.Now.AddDays(-10), DateTime.Now);
                    
                    ((ICommunicationObject)client).Close();
                }
                catch (CommunicationException ex)
                {
                    Console.WriteLine($"Communication Exception: {ex.Message}");
                    Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                    ((ICommunicationObject)client).Abort();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Error: {ex.InnerException.Message}");
            }
        }
    }
    
    static async Task GetCurrentRate(ICurrencyService client, string code)
    {
        try
        {
            decimal rate = await client.GetCurrentRateAsync(code);
            
            Console.WriteLine($"Current Exchange Rate");
            Console.WriteLine($"--------------------");
            Console.WriteLine($"Currency: {code}");
            Console.WriteLine($"Rate: 1 {code} = {rate} PLN");
            Console.WriteLine($"Date: {DateTime.Now.ToShortDateString()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting current rate: {ex.Message}");
        }
    }
    
    static async Task TestHistoricalRates(ICurrencyService client, string code, 
        DateTime startDate, DateTime endDate)
    {
        try
        {
            var history = await client.GetHistoricalRatesAsync(code, startDate, endDate);
            
            Console.WriteLine($"Historical {code} rates from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}:");
            Console.WriteLine($"--------------------");
            foreach (var r in history)
                Console.WriteLine($"{r.Date:yyyy-MM-dd}: {r.Mid}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting historical rates: {ex.Message}");
        }
    }
    
    static async Task TestBuySellRate(ICurrencyService client, string code)
    {
        try
        {
            var bs = await client.GetBuySellRateAsync(code);
            
            Console.WriteLine($"{code} Buy/Sell Rates");
            Console.WriteLine($"--------------------");
            Console.WriteLine($"Date: {bs.Date:yyyy-MM-dd}");
            Console.WriteLine($"Buy (Bid): {bs.Bid}");
            Console.WriteLine($"Sell (Ask): {bs.Ask}");
            Console.WriteLine($"Spread: {bs.Ask - bs.Bid:F4}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting buy/sell rate: {ex.Message}");
        }
    }
    
    static async Task TestGoldPrice(ICurrencyService client)
    {
        try
        {
            var goldNow = await client.GetCurrentGoldPriceAsync();
            
            Console.WriteLine($"Current Gold Price");
            Console.WriteLine($"------------------");
            Console.WriteLine($"Date: {goldNow.Date:yyyy-MM-dd}");
            Console.WriteLine($"Price: {goldNow.Price} PLN/g");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting gold price: {ex.Message}");
        }
    }
    
    static async Task TestHistoricalGoldPrices(ICurrencyService client, 
        DateTime startDate, DateTime endDate)
    {
        try
        {
            var goldHist = await client.GetHistoricalGoldPricesAsync(startDate, endDate);
            
            Console.WriteLine($"Historical Gold Prices from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}:");
            Console.WriteLine($"--------------------");
            foreach (var g in goldHist)
                Console.WriteLine($"{g.Date:yyyy-MM-dd}: {g.Price} PLN/g");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting historical gold prices: {ex.Message}");
        }
    }
}