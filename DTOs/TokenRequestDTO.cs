namespace JWTAuthenticationAPI.DTOs
{
    public class TokenRequestDTO
    {
        public required string RefreshToken { get; set; }
        public required string AccessToken { get; set; }
    }
}
