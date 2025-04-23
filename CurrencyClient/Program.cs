using System;
using System.ServiceModel;
using System.Threading.Tasks;
using CurrencyClient;

[ServiceContract(Name = "IService", Namespace = "http://currency.service")]
public interface ICurrencyService
{
    [OperationContract]
    Task<decimal> GetCurrentRateAsync(string code);
}

public class CurrencyServiceClient : ClientBase<ICurrencyService>, ICurrencyService
{
    public CurrencyServiceClient(System.ServiceModel.Channels.Binding binding, EndpointAddress address) 
        : base(binding, address) { }
    
    public Task<decimal> GetCurrentRateAsync(string code)
    {
        return Channel.GetCurrentRateAsync(code);
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
                    
                    Console.WriteLine("Calling GetCurrentRateAsync...");
                    await GetCurrentRate(client, "USD");
                    
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
            
            Console.WriteLine($"\nCurrent Exchange Rate");
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
}