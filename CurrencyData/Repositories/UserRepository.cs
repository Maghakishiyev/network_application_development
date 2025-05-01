using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Security.Cryptography;
using System.Text;

namespace CurrencyData.Repositories
{
    public class UserRepository
    {
        private readonly IMongoCollection<CurrencyData.User> _users;

        public UserRepository(IMongoDatabase database)
        {
            _users = database.GetCollection<CurrencyData.User>("users");
        }

        public async Task<User?> GetByIdAsync(ObjectId id)
        {
            return await _users.Find(u => u.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _users.Find(u => u.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task<User> CreateUserAsync(string email, string password)
        {
            // Validate inputs
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Email and password cannot be empty");
            }

            // Check if user already exists
            var existingUser = await GetByEmailAsync(email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("A user with this email already exists");
            }

            // Create new user
            var newUser = new User
            {
                Id = ObjectId.GenerateNewId(),
                Email = email,
                Username = email.Split('@')[0], // Simple username derivation
                PasswordHash = HashPassword(password),
                CreatedAt = DateTime.UtcNow
            };

            await _users.InsertOneAsync(newUser);
            return newUser;
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            var user = await GetByEmailAsync(email);
            if (user == null)
            {
                return null;
            }

            if (VerifyPassword(password, user.PasswordHash))
            {
                return user;
            }

            return null;
        }

        // Simple SHA-256 password hashing
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            string hashedPassword = HashPassword(password);
            return hashedPassword == storedHash;
        }
    }
}