using JWTAuthenticationAPI.Models;

namespace JWTAuthenticationAPI.Repository
{
    public interface IUserRepository
    {
        Task<int> CreateAsync(User user);
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> ExistsByEmailAsync(string email);
    }
}