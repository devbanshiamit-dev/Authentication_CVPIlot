using JWTAuthenticationAPI.Models;
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

        public async Task<int> RegisterAsync(User user)
        {
            var existingUser = await _repo.GetByEmailAsync(user.Email);

            if (existingUser != null)
                throw new Exception("User already exists.");

            return await _repo.CreateAsync(user);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _repo.GetByEmailAsync(email);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _repo.ExistsByEmailAsync(email);
        }

        public async Task<string> GenerateRefreshTokenAsync(int userId)
        {
            return await _refreshTokenService.GenerateAndStoreAsync(userId);
        }

        public async Task RevokeAllRefreshTokensAsync(int userId)
        {
            await _refreshTokenService.RevokeAllForUserAsync(userId);
        }
    }
}
