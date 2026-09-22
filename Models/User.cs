namespace JWTAuthenticationAPI.Models
{
    public class User
    {
        public Guid Id { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }      // never store plaintext

        public string Role { get; set; }               // "Admin" | "User"

        public bool IsEmailVerified { get; set; }
        public bool IsActive { get; set; }              // account enabled/disabled

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }              // soft delete flag
        public DateTime? DeletedAt { get; set; }
    }
}
