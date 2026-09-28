using JWTAuthenticationAPI.Exceptions;
using JWTAuthenticationAPI.JWT;
using JWTAuthenticationAPI.Models;
using JWTAuthenticationAPI.Repository;
using System.Security.Cryptography;
using System.Text;

namespace JWTAuthenticationAPI.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenRepository _repo;
        private readonly IConfiguration _con;
        private readonly JWTService _Jwt;
        private readonly int _refreshTokenDays;

        public RefreshTokenService(IRefreshTokenRepository repo, IConfiguration con, JWTService jwt)
        {
            _repo = repo;
            _con = con;
            _Jwt = jwt;

            var days = _con["JWT:RefreshTokenDays"]
                  ?? throw new InvalidOperationException("JWT:RefreshTokenDays is not configured in appsettings.json");

            _refreshTokenDays = int.Parse(days);
        }

        public async Task<string> GenerateAccessTokenAsync(string email, int Id)
        {
            return _Jwt.GenerateAccessToken(email, Id);
        }

        public async Task<string> GenerateAndStoreAsync(int userId)
        {
            var rawToken = _Jwt.GenerateRefreshToken();
            var hashed = Hash(rawToken);

            var entity = new RefreshToken
            {
                UserId = userId,
                Token = hashed,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenDays),
            };

            await _repo.CreateAsync(entity);
            return rawToken;
        }

        private async Task<RefreshToken?> ValidateAsync(string rawToken)
        {
            if (string.IsNullOrWhiteSpace(rawToken))
                return null;

            var hashed = Hash(rawToken);
            var stored = await _repo.GetByTokenAsync(hashed);

            if (stored == null || stored.IsRevoked)
                return null;

            if (stored.ExpiresAt <= DateTime.UtcNow)
                return null;

            return stored;
        }

        public async Task<RefreshToken> GetTokenByTokenAsync(string rawToken)
        {
            if (string.IsNullOrWhiteSpace(rawToken))
                throw new BadRequestException("Refresh token is required.");

            var hashed = Hash(rawToken);
            var refreshToken = await _repo.GetByTokenAsync(hashed);
            if (refreshToken == null)
            {
                throw new NotFoundException("Token Not Found In DB");
            }
            return refreshToken;
        }

        public async Task<string> RotateAsync(string oldRawToken)
        {
            var stored = await ValidateAsync(oldRawToken);

            if (stored == null)
                throw new UnauthorizedException("Token Is Invalid Or Expired");

            await RevokeAsync(oldRawToken);
            return await GenerateAndStoreAsync(stored.UserId);
        }

        public async Task RevokeAsync(string rawToken)
        {
            if (string.IsNullOrWhiteSpace(rawToken))
            {
                throw new BadRequestException("Refresh token is required.");
            }
            var hashed = Hash(rawToken);
            await _repo.RevokeAsync(hashed);
        }

        public async Task RevokeAllForUserAsync(int userId)
        {
            await _repo.RevokeAllForUserAsync(userId);
        }

        private static string Hash(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }
    }
}
