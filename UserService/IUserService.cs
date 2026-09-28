using JWTAuthenticationAPI.DTOs;
using JWTAuthenticationAPI.Models;

namespace JWTAuthenticationAPI.UserService
{
    public interface IUserService
    {
        Task<ServerResponse> RegisterAsync(UserRegistreRequest user);
        Task<ServerResponse> LoginRequestAsync(UserLoginRequest dto);
        Task<ServerResponse> RotateTokensAsync(TokenRequestDTO dTO);
        Task<string> GetAccessTokenAsync(string refreshToken);
        Task RevokeToken(string refreshToken);
        Task RevokeAllRefreshTokensAsync(int userId);
    }
}