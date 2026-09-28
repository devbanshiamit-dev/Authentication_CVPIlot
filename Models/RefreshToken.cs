namespace JWTAuthenticationAPI.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Token { get; set; } = string.Empty;
        public bool IsRevoked { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}
