namespace JWTAuthenticationAPI.Services
{
    public interface IRefreshTokenService
    {
        Task<string> GenerateAndStoreAsync(Guid userId, string jwtId, string? ipAddress);
        Task<(bool IsValid, Guid? UserId)> ValidateAsync(string rawToken);
        Task<string> RotateAsync(string oldRawToken, string newJwtId, string? ipAddress);
        Task RevokeAsync(string rawToken, string? ipAddress);
        Task RevokeAllForUserAsync(Guid userId);
    }
}
