using System.ComponentModel.DataAnnotations;

namespace JWTAuthenticationAPI.DTOs
{
    public class UserRegistreRequest
    {
        [Required, MaxLength(100)]
        public string name { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string Password { get; set; } = string.Empty;
    }
}
