using JWTAuthenticationAPI.DTOs;
using JWTAuthenticationAPI.UserService;
using Microsoft.AspNetCore.Authorization;
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

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> CreateAsync(UserRegistreRequest request)
        {
            var Response = await _user.RegisterAsync(request);
            return Ok(Response);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(UserLoginRequest request)
        {
            var Response = await _user.LoginRequestAsync(request);
            return Ok(Response);
        }

        [AllowAnonymous]
        [HttpPost("token")]
        public async Task<IActionResult> TokenRotateAsync([FromBody]RefreshTokenRequest request)
        {
            var Response = await _user.RotateTokensAsync(request.RefreshToken);
            return Ok(Response);
        }

        [AllowAnonymous]
        [HttpGet("access")]
        public async Task<IActionResult> GetAccessAsync(RefreshTokenRequest refresh)
        {
            var responce = await _user.GetAccessTokenAsync(refresh.RefreshToken);
            return Ok(responce);
        }

        [Authorize]
        [HttpDelete("Revoke")]
        public async Task<IActionResult> RevokeTokenAsync([FromBody]RefreshTokenRequest refresh)
        {
            int userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!await _user.RevokeToken(refresh.RefreshToken, userId))
            {
                return Forbid();
            }
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