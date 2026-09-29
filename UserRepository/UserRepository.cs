using JWTAuthenticationAPI.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace JWTAuthenticationAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found.");
        }

        public async Task<int> CreateAsync(User user)
        {
            const string query = @"
                INSERT INTO Users (Name, Email, PasswordHash)
                OUTPUT INSERTED.Id
                VALUES (@Name, @Email, @PasswordHash)";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@Name", SqlDbType.NVarChar, 100)
                .Value = user.Name;
            command.Parameters.Add("@Email", SqlDbType.NVarChar, 256)
                .Value = user.Email;

            command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 500)
                .Value = user.PasswordHash;

            await connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            const string query = @"
                SELECT Id, Name, Email, PasswordHash, CreatedAt
                FROM Users
                WHERE Id = @Id";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@Id", SqlDbType.Int)
                .Value = id;

            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new User
            {
                UserId = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            const string query = @"
                SELECT Id, Name, Email, PasswordHash, CreatedAt
                FROM Users
                WHERE Email = @Email";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@Email", SqlDbType.NVarChar, 256)
                .Value = email;

            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new User
            {
                UserId = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            const string query = @"
                SELECT COUNT(1)
                FROM Users
                WHERE Email = @Email";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@Email", SqlDbType.NVarChar, 256)
                .Value = email;

            await connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();

            return Convert.ToInt32(result) > 0;
        }
    }
}