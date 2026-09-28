using JWTAuthenticationAPI.Models;

namespace JWTAuthenticationAPI.UserService
{
    public interface IUserService
    {
        Task<int> RegisterAsync(User user);
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> ExistsByEmailAsync(string email);
        Task<string> GenerateRefreshTokenAsync(int userId);
        Task RevokeAllRefreshTokensAsync(int userId);
    }
}