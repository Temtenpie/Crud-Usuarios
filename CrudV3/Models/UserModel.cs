using System.Text.Json.Serialization;

namespace CrudV3.Models
{
    public class UserModel
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; } = true;

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("last_login")]
        public DateTime? LastLogin { get; set; }
    }
}