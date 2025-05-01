using System;

namespace CurrencyData.Models
{
    public class UserDto
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}