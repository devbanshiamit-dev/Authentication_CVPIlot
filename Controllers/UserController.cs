using JWTAuthenticationAPI.DTOs;
using JWTAuthenticationAPI.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JWTAuthenticationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _user;
        public UserController(IUserService user)
        {
            _user = user;
        }

        [HttpPost("register")]
        public async Task<IActionResult> CreateAsync(UserRegistreRequest request)
        {
            var Response = await _user.RegisterAsync(request);
            return Ok(Response);
        }
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(UserLoginRequest request)
        {
            var Response = await _user.LoginRequestAsync(request);
            return Ok(Response);
        }
        [HttpPost("token")]
        public async Task<IActionResult> TokenRotateAsync(TokenRequestDTO request)
        {
            var Response = await _user.RotateTokensAsync(request);
            return Ok(Response);
        }
        [HttpGet("access")]
        public async Task<IActionResult> GetAccessAsync(string refreshToken)
        {
            var responce = await _user.GetAccessTokenAsync(refreshToken);
            return Ok(responce);
        }
        [Authorize]
        [HttpDelete("Revoke")]
        public async Task<IActionResult> RevokeTokenAsync(string refreshToken)
        {
            await _user.RevokeToken(refreshToken);
            return NoContent();
        }

        [Authorize]
        [HttpDelete("revoke-from-all")]
        public async Task<IActionResult> RevokeAlltokenAsync()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
                return Unauthorized();

            await _user.RevokeAllRefreshTokensAsync(userId);
            return NoContent();
        }
    }
}
