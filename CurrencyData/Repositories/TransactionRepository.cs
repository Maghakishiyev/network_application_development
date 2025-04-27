using System.Threading.Tasks;
using MongoDB.Driver;

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
    }
}