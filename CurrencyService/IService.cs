using CoreWCF;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using CurrencyData.Models;
using CurrencyData;

namespace CurrencyService
{
    [ServiceContract(Namespace = "http://currency.service")]
    public interface IService
    {
        [OperationContract]
        string GetData(int value);

        [OperationContract]
        CompositeType GetDataUsingDataContract(CompositeType composite);

        /// <summary>
        /// Authenticates a user with email and password
        /// </summary>
        [OperationContract]
        Task<UserDto> AuthenticateAsync(string email, string password);
        
        /// <summary>
        /// Registers a new user with email and password
        /// </summary>
        [OperationContract]
        Task<UserDto> RegisterUserAsync(string email, string password);

        [OperationContract]
        Task<decimal> GetCurrentRateAsync(string code);
        
        /// <summary>
        /// Gets a series of "mid" exchange rates for the given currency between two dates (inclusive).
        /// </summary>
        [OperationContract]
        Task<RateDto[]> GetHistoricalRatesAsync(
            string code,
            DateTime startDate,
            DateTime endDate);
            
        /// <summary>
        /// Gets the current buy (bid) and sell (ask) prices for the given currency (table C).
        /// </summary>
        [OperationContract]
        Task<BuySellDto> GetBuySellRateAsync(string code);
        
        [OperationContract]
        Task<GoldDto> GetCurrentGoldPriceAsync();

        [OperationContract]
        Task<GoldDto[]> GetHistoricalGoldPricesAsync(
            DateTime startDate, DateTime endDate);
            
        /// <summary>
        /// Gets a user's account balances for all currencies.
        /// </summary>
        [OperationContract]
        Task<AccountDto> GetAccountAsync(string userId);
        
        /// <summary>
        /// Buys foreign currency using PLN.
        /// </summary>
        [OperationContract]
        Task<TradeResultDto> BuyCurrencyAsync(
            string userId,
            string currencyCode,
            decimal amountPln);
            
        /// <summary>
        /// Sells foreign currency to receive PLN.
        /// </summary>
        [OperationContract]
        Task<TradeResultDto> SellCurrencyAsync(
            string userId,
            string currencyCode,
            decimal amountForeign);
            
        /// <summary>
        /// Gets transaction history for a user.
        /// </summary>
        [OperationContract]
        Task<CurrencyData.Transaction[]> GetTransactionsAsync(string userId);
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
