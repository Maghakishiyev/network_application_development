using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;

namespace CurrencyData.Repositories
{
    public class BalanceRepository
    {
        private readonly IMongoCollection<Balance> _balances;

        public BalanceRepository(IMongoDatabase db)
        {
            _balances = db.GetCollection<Balance>("UserBalances");
        }

        // Get the user's balance document (one document per user with all currencies)
        public async Task<Balance> GetUserBalanceAsync(ObjectId userId)
        {
            var filter = Builders<Balance>.Filter.Eq(b => b.UserId, userId);
            var existing = await _balances.Find(filter).FirstOrDefaultAsync();
            
            if (existing != null)
                return existing;
                
            // Create a new balance with a generated ID if not found
            var newBalance = new Balance 
            { 
                Id = ObjectId.GenerateNewId(),
                UserId = userId,
                Currencies = new Dictionary<string, decimal>()
            };
            
            // We don't need to save it right away, that will happen when there's an actual
            // balance change via UpdateBalanceAsync
            return newBalance;
        }
        
        // Get amount for a specific currency
        public async Task<decimal> GetAmountAsync(ObjectId userId, string currencyCode)
        {
            var balance = await GetUserBalanceAsync(userId);
            return balance.GetAmount(currencyCode);
        }
        
        // Update a specific currency balance
        public async Task UpdateBalanceAsync(ObjectId userId, string currencyCode, decimal amount)
        {
            // Get the current balance document
            var balance = await GetUserBalanceAsync(userId);
            
            // Update the amount for the specific currency
            balance.SetAmount(currencyCode, amount);
            
            // Save the updated balance document
            await UpdateUserBalanceAsync(balance);
        }
        
        // Adjust a currency balance by adding/subtracting delta
        public async Task AdjustBalanceAsync(ObjectId userId, string currencyCode, decimal delta)
        {
            var balance = await GetUserBalanceAsync(userId);
            balance.AdjustAmount(currencyCode, delta);
            await UpdateUserBalanceAsync(balance);
        }
        
        // Save a balance document
        private Task UpdateUserBalanceAsync(Balance balance)
        {
            // Ensure the balance has a valid ID
            if (balance.Id == ObjectId.Empty)
            {
                balance.Id = ObjectId.GenerateNewId();
            }
            
            var filter = Builders<Balance>.Filter.Eq(b => b.UserId, balance.UserId);
            return _balances.ReplaceOneAsync(filter, balance, new ReplaceOptions { IsUpsert = true });
        }
        
        // Create a new balance document
        public async Task CreateAsync(Balance balance)
        {
            // Make sure we have a valid ID
            if (balance.Id == ObjectId.Empty)
            {
                balance.Id = ObjectId.GenerateNewId();
            }
            
            // Check if a balance already exists for this user
            var existingFilter = Builders<Balance>.Filter.Eq(b => b.UserId, balance.UserId);
            var existing = await _balances.Find(existingFilter).FirstOrDefaultAsync();
            
            if (existing != null)
            {
                // Update existing balance
                await _balances.ReplaceOneAsync(existingFilter, balance);
            }
            else
            {
                // Insert new balance
                await _balances.InsertOneAsync(balance);
            }
        }
    }
}