using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CurrencyClient
{
    public class WcfTestClient
    {
        public static async Task TestWcfService()
        {
            Console.WriteLine("Direct SOAP request test");
            Console.WriteLine("========================");
            
            string soapEnvelope = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:a=""http://currency.service"">
                <s:Header />
                <s:Body>
                    <a:GetCurrentRateAsync>
                        <a:code>USD</a:code>
                    </a:GetCurrentRateAsync>
                </s:Body>
            </s:Envelope>";
            
            string serviceUrl = "http://localhost:5000/";
            
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("SOAPAction", "http://currency.service/IService/GetCurrentRateAsync");
                
                var content = new StringContent(soapEnvelope, System.Text.Encoding.UTF8, "text/xml");
                Console.WriteLine("Sending SOAP request to " + serviceUrl);
                
                var response = await client.PostAsync(serviceUrl, content);
                string responseContent = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine("Response status: " + response.StatusCode);
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Success! Response:");
                    try
                    {
                        var doc = XDocument.Parse(responseContent);
                        Console.WriteLine(doc.ToString());
                        
                        var ns = XNamespace.Get("http://schemas.xmlsoap.org/soap/envelope/");
                        var bodyElement = doc.Descendants(ns + "Body").FirstOrDefault();
                        
                        if (bodyElement != null)
                        {
                            var responseElement = bodyElement.Elements().FirstOrDefault();
                            if (responseElement != null)
                            {
                                var resultElement = responseElement.Elements().FirstOrDefault();
                                if (resultElement != null)
                                {
                                    Console.WriteLine($"Extracted rate: {resultElement.Value}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error parsing response: " + ex.Message);
                        Console.WriteLine("Raw response: " + responseContent);
                    }
                }
                else
                {
                    Console.WriteLine("Error response: " + responseContent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Error: " + ex.InnerException.Message);
                }
            }
        }
    }
}