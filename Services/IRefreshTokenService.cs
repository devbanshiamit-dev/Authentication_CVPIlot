namespace JWTAuthenticationAPI.Services
{
    public interface IRefreshTokenService
    {
        Task<string> GenrateAccess(string email, int Id);
        Task<string> GenerateAndStoreAsync(int userId);
        Task<string> RotateAsync(string oldRawToken);
        Task RevokeAsync(string rawToken);
        Task RevokeAllForUserAsync(int userId);
    }
}
