namespace JWTAuthenticationAPI.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public Guid UserId { get; set; }              // FK -> Users.Id
        public string Token { get; set; }              // the refresh token value (store hashed, not raw)
        public string JwtId { get; set; }               // ties refresh token to the access token that issued it (jti claim)

        public bool IsUsed { get; set; }                 // one-time use flag
        public bool IsRevoked { get; set; }              // manually invalidated (logout, security event)

        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        public string CreatedByIp { get; set; }        // optional, useful for auditing
        public string RevokedByIp { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => !IsRevoked && !IsUsed && !IsExpired;
    }
}
