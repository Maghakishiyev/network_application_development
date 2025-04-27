using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Bson;

namespace CurrencyData.Repositories
{
    public class TransactionRepository
    {
        private readonly IMongoCollection<Transaction> _txns;

        public TransactionRepository(IMongoDatabase db)
        {
            _txns = db.GetCollection<Transaction>("Transactions");
        }

        public Task CreateAsync(Transaction tx) =>
            _txns.InsertOneAsync(tx);
            
        public async Task<List<Transaction>> GetByUserIdAsync(ObjectId userId)
        {
            var filter = Builders<Transaction>.Filter.Eq(t => t.UserId, userId);
            return await _txns.Find(filter).SortByDescending(t => t.Timestamp).ToListAsync();
        }
    }
}