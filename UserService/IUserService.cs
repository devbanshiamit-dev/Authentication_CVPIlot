using JWTAuthenticationAPI.DTOs;
using JWTAuthenticationAPI.Models;

namespace JWTAuthenticationAPI.UserService
{
    public interface IUserService
    {
        Task<ServerResponse> RegisterAsync(UserRegistreRequest user);
        Task<ServerResponse> LoginRequestAsync(UserLoginRequest dto);
        Task<ServerResponse> RotateTokensAsync(string refreshToken);
        Task<string> GetAccessTokenAsync(string refreshToken);
        Task<bool> RevokeToken(string refreshToken, int userid);
        Task RevokeAllRefreshTokensAsync(int userId);
    }
}