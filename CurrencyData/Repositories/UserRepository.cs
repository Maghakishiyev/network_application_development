using System;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace CurrencyData.Repositories
{
    public class UserRepository
    {
        private readonly IMongoCollection<CurrencyData.User> _users;

        public UserRepository(IMongoDatabase database)
        {
            _users = database.GetCollection<CurrencyData.User>("users");
        }

        // Add repository methods as needed
    }
}