using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace JWTAuthenticationAPI.JWT
{
    public class JWTService
    {
        private readonly IConfiguration _configuration;

        public JWTService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateAccessToken(
            string email,
            int userId)
        {
            var key = _configuration["JWT:ServerKey"]!;
            var issuer = _configuration["JWT:Issuer"]!;
            var audience = _configuration["JWT:Audience"]!;

            var minutes = int.Parse(
                _configuration["JWT:AccessTokenMinutes"] ?? "15"
            );

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            };

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key)
            );

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(minutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);

            return Convert.ToBase64String(randomBytes);
        }

        public DateTime GetRefreshTokenExpiry()
        {
            var days = int.Parse(
                _configuration["JWT:RefreshTokenDays"] ?? "7"
            );

            return DateTime.UtcNow.AddDays(days);
        }

        public TokenValidationParameters CreateValidationParameters()
        {
            var key = _configuration["JWT:ServerKey"]!;
            var issuer = _configuration["JWT:Issuer"]!;
            var audience = _configuration["JWT:Audience"]!;

            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        }

        public ClaimsPrincipal? ValidateAccessToken(
            string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(
                    accessToken,
                    CreateValidationParameters(),
                    out var validatedToken
                );

                if (validatedToken is not JwtSecurityToken jwt ||
                    !jwt.Header.Alg.Equals(
                        SecurityAlgorithms.HmacSha256,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch (SecurityTokenException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}
