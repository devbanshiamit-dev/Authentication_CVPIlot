namespace JWTAuthenticationAPI.DTOs
{
    public class ServerResponse
    {
        public string RefreshToken { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
    }
}
