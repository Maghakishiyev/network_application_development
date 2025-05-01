using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CoreWCF;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using CurrencyData.Models;
using CurrencyData.Repositories;
using MongoDB.Bson;
using CurrencyData;

namespace CurrencyService
{
    public class Service : IService
    {
        private readonly UserRepository _users;
        private readonly BalanceRepository _balances;
        private readonly TransactionRepository _txns;
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<Service> _log;

        public Service(
            UserRepository users,
            BalanceRepository balances,
            TransactionRepository txns,
            IHttpClientFactory httpFactory,
            ILogger<Service> log
        )
        {
            _users = users;
            _balances = balances;
            _txns = txns;
            _httpFactory = httpFactory;
            _log = log;
        }
        
        public async Task<UserDto> AuthenticateAsync(string email, string password)
        {
            try
            {
                _log.LogInformation("Authenticating user with email: {Email}", email);
                
                var user = await _users.AuthenticateAsync(email, password);
                if (user == null)
                {
                    _log.LogWarning("Authentication failed for email: {Email}", email);
                    throw new FaultException("Invalid email or password");
                }
                
                _log.LogInformation("User authenticated successfully: {Email}", email);
                
                return new UserDto
                {
                    Id = user.Id.ToString(),
                    Email = user.Email,
                    Username = user.Username,
                    CreatedAt = user.CreatedAt
                };
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error in AuthenticateAsync: {Message}", ex.Message);
                throw new FaultException($"Authentication error: {ex.Message}");
            }
        }
        
        public async Task<UserDto> RegisterUserAsync(string email, string password)
        {
            try
            {
                _log.LogInformation("Registering new user with email: {Email}", email);
                
                // Validate input
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    throw new FaultException("Email and password are required");
                }
                
                if (password.Length < 6)
                {
                    throw new FaultException("Password must be at least 6 characters long");
                }
                
                // Create user
                var user = await _users.CreateUserAsync(email, password);
                
                // Initialize an empty balance for the new user
                await _balances.CreateAsync(new Balance
                {
                    Id = ObjectId.GenerateNewId(),
                    UserId = user.Id,
                    Currencies = new Dictionary<string, decimal>
                    {
                        { "PLN", 0 }
                    }
                });
                
                _log.LogInformation("User registered successfully: {Email}, {UserId}", email, user.Id);
                
                return new UserDto
                {
                    Id = user.Id.ToString(),
                    Email = user.Email,
                    Username = user.Username,
                    CreatedAt = user.CreatedAt
                };
            }
            catch (InvalidOperationException ex)
            {
                _log.LogWarning("Registration failed: {Message}", ex.Message);
                throw new FaultException(ex.Message);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error in RegisterUserAsync: {Message}", ex.Message);
                throw new FaultException($"Registration error: {ex.Message}");
            }
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
            // Special case: PLN is the base currency
            if (code.Equals("PLN", StringComparison.OrdinalIgnoreCase))
            {
                _log.LogInformation("PLN is the base currency, rate is 1.0");
                return 1.0m;
            }
            
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
            // Special case: PLN is the base currency
            if (code.Equals("PLN", StringComparison.OrdinalIgnoreCase))
            {
                var plnRate = new BuySellDto
                {
                    Date = DateTime.Today,
                    Bid = 1.0m,
                    Ask = 1.0m
                };
                
                _log.LogInformation("PLN is the base currency, bid=1.0, ask=1.0");
                return plnRate;
            }
            
            var url = $"https://api.nbp.pl/api/exchangerates/rates/c/{code}/?format=json";
            var client = _httpFactory.CreateClient();
            var resp = await client.GetAsync(url);
            resp.EnsureSuccessStatusCode();

            using var stream = await resp.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            var root = doc.RootElement.GetProperty("rates")[0];

            var foreignRate = new BuySellDto
            {
                Date = root.GetProperty("effectiveDate").GetDateTime(),
                Bid = root.GetProperty("bid").GetDecimal(),
                Ask = root.GetProperty("ask").GetDecimal()
            };
            
            _log.LogInformation("Fetched buy/sell rate for {Code}: bid={Bid}, ask={Ask}", 
                code, foreignRate.Bid, foreignRate.Ask);
            return foreignRate;
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
        
        public async Task<AccountDto> GetAccountAsync(string userId)
        {
            var uid = ObjectId.Parse(userId);
            
            // Get all user balances in one go with our new model
            var balance = await _balances.GetUserBalanceAsync(uid);

            _log.LogInformation("Fetched balances for user {UserId}", userId);
            
            // Create a new dictionary to be safe - we don't want to expose
            // our internal object directly
            var balanceDict = new Dictionary<string, decimal>();
            
            // Make sure we at least show USD, EUR, and PLN even if zero
            balanceDict["USD"] = balance.GetAmount("USD");
            balanceDict["EUR"] = balance.GetAmount("EUR");
            balanceDict["PLN"] = balance.GetAmount("PLN");
            
            // Add any other currencies the user might have
            foreach (var pair in balance.Currencies)
            {
                if (!balanceDict.ContainsKey(pair.Key))
                {
                    balanceDict[pair.Key] = pair.Value;
                }
            }
            
            return new AccountDto
            {
                UserId = userId,
                Balances = balanceDict
            };
        }
        
        public async Task<TradeResultDto> BuyCurrencyAsync(
            string userId, string currencyCode, decimal amountPln)
        {
            var uid = ObjectId.Parse(userId);
            var timestamp = DateTime.UtcNow;
            
            try
            {
                // Special handling for PLN (for initial balance/top-up)
                if (currencyCode.Equals("PLN", StringComparison.OrdinalIgnoreCase))
                {
                    // Add to PLN balance directly
                    await _balances.AdjustBalanceAsync(uid, "PLN", amountPln);
                    
                    // Log and return
                    _log.LogInformation("User {UserId} deposited {Amount} PLN (initial balance)", userId, amountPln);
                    
                    return new TradeResultDto
                    {
                        UserId = userId,
                        CurrencyCode = "PLN",
                        AmountForeign = amountPln,
                        AmountPln = amountPln,
                        Rate = 1.0m,
                        Timestamp = timestamp
                    };
                }
                
                // Normal currency purchase
                var rate = await GetCurrentRateAsync(currencyCode);
                var amountForeign = amountPln / rate;
    
                // Check PLN balance is sufficient
                var plnBalance = await _balances.GetAmountAsync(uid, "PLN");
                if (plnBalance < amountPln)
                {
                    throw new InvalidOperationException($"Insufficient PLN balance: {plnBalance} < {amountPln}");
                }
                
                // Update balances in one transaction
                await _balances.AdjustBalanceAsync(uid, "PLN", -amountPln);
                await _balances.AdjustBalanceAsync(uid, currencyCode, amountForeign);
    
                // Record transaction
                try 
                {
                    var tx = new CurrencyData.Transaction
                    {
                        Id = ObjectId.GenerateNewId(),
                        UserId = uid,
                        Type = "buy",
                        CurrencyCode = currencyCode,
                        Amount = amountForeign,
                        Timestamp = timestamp
                    };
                    await _txns.CreateAsync(tx);
                }
                catch (Exception ex)
                {
                    // Log but don't fail if transaction recording fails
                    _log.LogWarning("Failed to record transaction: {Error}", ex.Message);
                }
    
                _log.LogInformation("User {UserId} bought {Amount} {Currency} for {Pln} PLN at {Rate}", 
                    userId, amountForeign, currencyCode, amountPln, rate);
                    
                return new TradeResultDto
                {
                    UserId = userId,
                    CurrencyCode = currencyCode,
                    AmountForeign = amountForeign,
                    AmountPln = amountPln,
                    Rate = rate,
                    Timestamp = timestamp
                };
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error in BuyCurrencyAsync: {Message}", ex.Message);
                throw;
            }
        }
        
        public async Task<TradeResultDto> SellCurrencyAsync(
            string userId, string currencyCode, decimal amountForeign)
        {
            var uid = ObjectId.Parse(userId);
            var timestamp = DateTime.UtcNow;
            
            try
            {
                // Special handling for PLN
                if (currencyCode.Equals("PLN", StringComparison.OrdinalIgnoreCase))
                {
                    // For the PLN case, just return a simulated result without actual withdrawal
                    _log.LogInformation("User {UserId} simulated withdrawal of {Amount} PLN", userId, amountForeign);
                    
                    return new TradeResultDto
                    {
                        UserId = userId,
                        CurrencyCode = "PLN",
                        AmountForeign = amountForeign,
                        AmountPln = amountForeign,
                        Rate = 1.0m,
                        Timestamp = timestamp
                    };
                }
                
                // Normal sell operation
                var rate = await GetCurrentRateAsync(currencyCode);
                var amountPln = amountForeign * rate;
    
                // Check foreign currency balance
                var foreignBalance = await _balances.GetAmountAsync(uid, currencyCode);
                if (foreignBalance < amountForeign)
                {
                    throw new InvalidOperationException($"Insufficient {currencyCode} balance: {foreignBalance} < {amountForeign}");
                }
                
                // Update balances in one transaction
                await _balances.AdjustBalanceAsync(uid, currencyCode, -amountForeign);
                await _balances.AdjustBalanceAsync(uid, "PLN", amountPln);
    
                // Record transaction
                try 
                {
                    var tx = new CurrencyData.Transaction
                    {
                        Id = ObjectId.GenerateNewId(),
                        UserId = uid,
                        Type = "sell",
                        CurrencyCode = currencyCode,
                        Amount = amountForeign,
                        Timestamp = timestamp
                    };
                    await _txns.CreateAsync(tx);
                }
                catch (Exception ex) 
                {
                    // Log but don't fail if transaction recording fails
                    _log.LogWarning("Failed to record transaction: {Error}", ex.Message);
                }
    
                _log.LogInformation("User {UserId} sold {Amount} {Currency} for {Pln} PLN at {Rate}", 
                    userId, amountForeign, currencyCode, amountPln, rate);
                    
                return new TradeResultDto
                {
                    UserId = userId,
                    CurrencyCode = currencyCode,
                    AmountForeign = amountForeign,
                    AmountPln = amountPln,
                    Rate = rate,
                    Timestamp = timestamp
                };
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error in SellCurrencyAsync: {Message}", ex.Message);
                throw;
            }
        }
        
        public async Task<CurrencyData.Transaction[]> GetTransactionsAsync(string userId)
        {
            try
            {
                var uid = ObjectId.Parse(userId);
                var transactions = await _txns.GetByUserIdAsync(uid);
                
                _log.LogInformation("Fetched {Count} transactions for user {UserId}", 
                    transactions.Count, userId);
                    
                return transactions.ToArray();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error fetching transactions: {Message}", ex.Message);
                throw;
            }
        }
    }
}