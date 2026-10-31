namespace JWTAuthenticationAPI.DTOs
{
    public class TokenRotationRequest
    {
        public required string RefreshToken { get; set; }
    }
}
