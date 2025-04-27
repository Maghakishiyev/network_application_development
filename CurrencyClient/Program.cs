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
    
    [OperationContract]
    Task<AccountDto> GetAccountAsync(string userId);
    
    [OperationContract]
    Task<TradeResultDto> BuyCurrencyAsync(string userId, string currencyCode, decimal amountPln);
    
    [OperationContract]
    Task<TradeResultDto> SellCurrencyAsync(string userId, string currencyCode, decimal amountForeign);
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
    
    public Task<AccountDto> GetAccountAsync(string userId)
    {
        return Channel.GetAccountAsync(userId);
    }
    
    public Task<TradeResultDto> BuyCurrencyAsync(string userId, string currencyCode, decimal amountPln)
    {
        return Channel.BuyCurrencyAsync(userId, currencyCode, amountPln);
    }
    
    public Task<TradeResultDto> SellCurrencyAsync(string userId, string currencyCode, decimal amountForeign)
    {
        return Channel.SellCurrencyAsync(userId, currencyCode, amountForeign);
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Currency Exchange WCF Client");
        Console.WriteLine("============================");
        
        // Generate a test user ID for MongoDB
        string userId = "650d01234567890123456789"; // Example ObjectId
        Console.WriteLine($"Test User ID: {userId}");
        
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
                    
                    // ==== Phase 3 Tests ====
                    
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
                    
                    // ==== Phase 4 Tests ====
                    
                    // 1. Get account (should be empty initially)
                    Console.WriteLine("\n=== ACCOUNT TEST - INITIAL STATE ===");
                    await TestGetAccount(client, userId);
                    
                    // 2. Set up PLN balance
                    Console.WriteLine("\n=== ACCOUNT TEST - TOP UP PLN ===");
                    await TopUpPlnManually(client, userId, 1000m);
                    
                    // 3. Check account after top-up
                    Console.WriteLine("\n=== ACCOUNT TEST - AFTER TOP-UP ===");
                    await TestGetAccount(client, userId);
                    
                    // 4. Buy some USD
                    Console.WriteLine("\n=== TRADING TEST - BUY USD ===");
                    await TestBuyCurrency(client, userId, "USD", 300m);
                    
                    // 5. Check account after purchase
                    Console.WriteLine("\n=== ACCOUNT TEST - AFTER BUYING USD ===");
                    await TestGetAccount(client, userId);
                    
                    // 6. Buy some EUR
                    Console.WriteLine("\n=== TRADING TEST - BUY EUR ===");
                    await TestBuyCurrency(client, userId, "EUR", 200m);
                    
                    // 7. Check account after EUR purchase
                    Console.WriteLine("\n=== ACCOUNT TEST - AFTER BUYING EUR ===");
                    await TestGetAccount(client, userId);
                    
                    // 8. Sell some USD
                    Console.WriteLine("\n=== TRADING TEST - SELL USD ===");
                    await TestSellCurrency(client, userId, "USD", 30m);
                    
                    // 9. Check final account state
                    Console.WriteLine("\n=== ACCOUNT TEST - FINAL STATE ===");
                    await TestGetAccount(client, userId);
                    
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
    
    // ===== Phase 3 Test Methods =====
    
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
    
    // ===== Phase 4 Test Methods =====
    
    // Method to add PLN to the account (initial deposit)
    static async Task TopUpPlnManually(ICurrencyService client, string userId, decimal amount)
    {
        try
        {
            // Simulate adding PLN directly
            Console.WriteLine($"Attempting to add {amount} PLN to account {userId}...");
            var result = await client.BuyCurrencyAsync(userId, "PLN", amount);
            Console.WriteLine($"✅ Success! Topped up {amount} PLN");
            Console.WriteLine($"Transaction ID: {result.Timestamp:yyyy-MM-dd HH:mm:ss}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Top-up operation failed: {ex.Message}");
            Console.WriteLine("Check MongoDB connection and database setup");
        }
    }
    
    static async Task TestGetAccount(ICurrencyService client, string userId)
    {
        try
        {
            var account = await client.GetAccountAsync(userId);
            
            Console.WriteLine($"Account Balances for User: {userId}");
            Console.WriteLine($"-------------------------{new string('-', Math.Min(userId.Length, 10))}");
            
            foreach (var balance in account.Balances)
            {
                Console.WriteLine($"{balance.Key}: {balance.Value}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting account: {ex.Message}");
        }
    }
    
    static async Task TestBuyCurrency(ICurrencyService client, string userId, string currencyCode, decimal amountPln)
    {
        try
        {
            var result = await client.BuyCurrencyAsync(userId, currencyCode, amountPln);
            
            Console.WriteLine($"Currency Purchase Result");
            Console.WriteLine($"------------------------");
            Console.WriteLine($"Currency: {result.CurrencyCode}");
            Console.WriteLine($"Amount Spent: {result.AmountPln} PLN");
            Console.WriteLine($"Amount Received: {result.AmountForeign} {result.CurrencyCode}");
            Console.WriteLine($"Exchange Rate: {result.Rate}");
            Console.WriteLine($"Transaction Time: {result.Timestamp}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error buying currency: {ex.Message}");
        }
    }
    
    static async Task TestSellCurrency(ICurrencyService client, string userId, string currencyCode, decimal amountForeign)
    {
        try
        {
            var result = await client.SellCurrencyAsync(userId, currencyCode, amountForeign);
            
            Console.WriteLine($"Currency Sale Result");
            Console.WriteLine($"--------------------");
            Console.WriteLine($"Currency: {result.CurrencyCode}");
            Console.WriteLine($"Amount Sold: {result.AmountForeign} {result.CurrencyCode}");
            Console.WriteLine($"Amount Received: {result.AmountPln} PLN");
            Console.WriteLine($"Exchange Rate: {result.Rate}");
            Console.WriteLine($"Transaction Time: {result.Timestamp}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error selling currency: {ex.Message}");
        }
    }
}