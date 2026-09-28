using JWTAuthenticationAPI.DTOs;
using JWTAuthenticationAPI.Exceptions;
using JWTAuthenticationAPI.Models;
using JWTAuthenticationAPI.PasswordSecurity;
using JWTAuthenticationAPI.Repository;
using JWTAuthenticationAPI.Services;

namespace JWTAuthenticationAPI.UserService
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IRefreshTokenService _refreshTokenService;

        public UserService(
            IUserRepository repo,
            IRefreshTokenService refreshTokenService)
        {
            _repo = repo;
            _refreshTokenService = refreshTokenService;
        }

        // New Registration
        public async Task<ServerResponse> RegisterAsync(UserRegistreRequest user)
        {
            var existingUser = await _repo.GetByEmailAsync(user.Email);

            if (existingUser != null)
                throw new ConflictException("User already exists.");

            var entity = new User
            {
                Name = user.name,
                Email = user.Email,
                PasswordHash = PasswordHasher.HashPassword(user.Password)
            };

            int id = await _repo.CreateAsync(entity);

            entity.UserId = id;

            var refreshtoken =
                await _refreshTokenService.GenerateAndStoreAsync(entity.UserId);

            var access =
                await _refreshTokenService.GenerateAccessTokenAsync(entity.Email, entity.UserId);

            return new ServerResponse
            {
                RefreshToken = refreshtoken,
                AccessToken = access
            };
        }

        // Login
        public async Task<ServerResponse> LoginRequestAsync(UserLoginRequest dto)
        {
            var stored = await _repo.GetByEmailAsync(dto.Email);

            if (stored == null)
                throw new UnauthorizedException("Invalid email or password.");

            if (!PasswordHasher.VerifyPassword(dto.Password, stored.PasswordHash))
                throw new UnauthorizedException("Invalid email or password.");

            var refreshtoken =
                await _refreshTokenService.GenerateAndStoreAsync(stored.UserId);

            var access =
                await _refreshTokenService.GenerateAccessTokenAsync(stored.Email, stored.UserId);

            return new ServerResponse
            {
                RefreshToken = refreshtoken,
                AccessToken = access
            };
        }

        //Token Rotation
        public async Task<ServerResponse> RotateTokensAsync(TokenRequestDTO dTO)
        {
            var storedToken =
                await _refreshTokenService.GetTokenByTokenAsync(dTO.RefreshToken);

            if (storedToken.IsRevoked)
                throw new UnauthorizedException("Token Is Revoked");

            if (storedToken.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedException("Token Expired");

            User stored = await _repo.GetByIdAsync(storedToken.UserId)
                ?? throw new NotFoundException("Failed to find user for this token");

            await _refreshTokenService.RevokeAsync(dTO.RefreshToken);

            var refreshtoken =
                await _refreshTokenService.GenerateAndStoreAsync(stored.UserId);

            var access =
                await _refreshTokenService.GenerateAccessTokenAsync(stored.Email, stored.UserId);

            return new ServerResponse
            {
                RefreshToken = refreshtoken,
                AccessToken = access
            };
        }

        public async Task<string> GetAccessTokenAsync(string refreshToken)
        {
            var storedToken =
                await _refreshTokenService.GetTokenByTokenAsync(refreshToken);

            if (storedToken.IsRevoked)
                throw new UnauthorizedException("Token Is Revoked");

            if (storedToken.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedException("Token Expired");

            User stored = await _repo.GetByIdAsync(storedToken.UserId)
                ?? throw new NotFoundException("Failed to find user for this token");

            var access =
               await _refreshTokenService.GenerateAccessTokenAsync(stored.Email, stored.UserId);

            return access;
        }

        public async Task RevokeToken(string refreshToken)
        {
            await _refreshTokenService.RevokeAsync(refreshToken);
        }

        public async Task RevokeAllRefreshTokensAsync(int userId)
        {
            await _refreshTokenService.RevokeAllForUserAsync(userId);
        }
    }
}
