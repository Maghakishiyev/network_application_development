using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using CurrencyData.Models;
using CurrencyData;

namespace CurrencyService
{
    // Generated reference class for the service contract
    [ServiceContract(Namespace = "http://currency.service")]
    public interface IService
    {
        [OperationContract]
        string GetData(int value);

        [OperationContract]
        CompositeType GetDataUsingDataContract(CompositeType composite);

        [OperationContract]
        Task<decimal> GetCurrentRateAsync(string code);
        
        [OperationContract]
        Task<RateDto[]> GetHistoricalRatesAsync(
            string code,
            DateTime startDate,
            DateTime endDate);
            
        [OperationContract]
        Task<BuySellDto> GetBuySellRateAsync(string code);
        
        [OperationContract]
        Task<GoldDto> GetCurrentGoldPriceAsync();

        [OperationContract]
        Task<GoldDto[]> GetHistoricalGoldPricesAsync(
            DateTime startDate, DateTime endDate);
            
        [OperationContract]
        Task<AccountDto> GetAccountAsync(string userId);
        
        [OperationContract]
        Task<TradeResultDto> BuyCurrencyAsync(
            string userId,
            string currencyCode,
            decimal amountPln);
            
        [OperationContract]
        Task<TradeResultDto> SellCurrencyAsync(
            string userId,
            string currencyCode,
            decimal amountForeign);

        [OperationContract]
        Task<Transaction[]> GetTransactionsAsync(string userId);
    }

    [DataContract]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }
}