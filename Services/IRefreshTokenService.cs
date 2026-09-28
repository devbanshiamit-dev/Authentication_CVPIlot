using JWTAuthenticationAPI.Models;

namespace JWTAuthenticationAPI.Services
{
    public interface IRefreshTokenService
    {
        Task<string> GenerateAccessTokenAsync(string email, int Id);
        Task<string> GenerateAndStoreAsync(int userId);
        Task<RefreshToken> GetTokenByTokenAsync(string Token);
        Task<string> RotateAsync(string oldRawToken);
        Task RevokeAsync(string rawToken);
        Task RevokeAllForUserAsync(int userId);
    }
}
