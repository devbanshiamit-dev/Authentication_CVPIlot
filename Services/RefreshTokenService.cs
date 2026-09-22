using JWTAuthenticationAPI.Models;
using JWTAuthenticationAPI.Repository;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace JWTAuthenticationAPI.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenRepository _repo;
        private const int ExpiryDays = 7;

        public RefreshTokenService(IRefreshTokenRepository repo)
        {
            _repo = repo;
        }

        public async Task<string> GenerateAndStoreAsync(Guid userId, string jwtId, string? ipAddress)
        {
            var rawToken = GenerateSecureToken();
            var hashed = Hash(rawToken);

            var entity = new RefreshToken
            {
                UserId = userId,
                Token = hashed,
                JwtId = jwtId,
                IsUsed = false,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(ExpiryDays),
                CreatedByIp = ipAddress
            };

            await _repo.CreateAsync(entity);
            return rawToken; // return raw token to client; only the hash is stored
        }

        public async Task<(bool IsValid, Guid? UserId)> ValidateAsync(string rawToken)
        {
            var hashed = Hash(rawToken);
            var stored = await _repo.GetByTokenAsync(hashed);

            if (stored == null || !stored.IsActive)
                return (false, null);

            return (true, stored.UserId);
        }

        public async Task<string> RotateAsync(string oldRawToken, string newJwtId, string? ipAddress)
        {
            var oldHashed = Hash(oldRawToken);
            var stored = await _repo.GetByTokenAsync(oldHashed);

            if (stored == null || !stored.IsActive)
                throw new SecurityTokenException("Invalid or reused refresh token.");

            // mark old one used (rotation), issue a new one
            await _repo.MarkUsedAsync(oldHashed);
            return await GenerateAndStoreAsync(stored.UserId, newJwtId, ipAddress);
        }

        public async Task RevokeAsync(string rawToken, string? ipAddress)
        {
            var hashed = Hash(rawToken);
            await _repo.RevokeAsync(hashed, ipAddress);
        }

        public async Task RevokeAllForUserAsync(Guid userId)
        {
            await _repo.RevokeAllForUserAsync(userId);
        }

        private static string GenerateSecureToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        private static string Hash(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }
    }
}
