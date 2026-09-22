using JWTAuthenticationAPI.Models;

namespace JWTAuthenticationAPI.Repository
{
    public interface IRefreshTokenRepository
    {
        Task<int> CreateAsync(RefreshToken token);
        Task<RefreshToken?> GetByTokenAsync(string tokenHash);
        Task RevokeAsync(string tokenHash, string revokedByIp);
        Task MarkUsedAsync(string tokenHash);
        Task RevokeAllForUserAsync(Guid userId);
    }
}
