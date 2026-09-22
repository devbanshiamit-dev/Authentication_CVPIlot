using JWTAuthenticationAPI.Models;
using Microsoft.Data.SqlClient;

namespace JWTAuthenticationAPI.Repository
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly string _connectionString;

        public RefreshTokenRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection String not Found");
        }

        public async Task<int> CreateAsync(RefreshToken token)
        {
            const string query = @"
            INSERT INTO RefreshTokens (UserId, Token, JwtId, IsUsed, IsRevoked, CreatedAt, ExpiresAt, CreatedByIp)
            OUTPUT INSERTED.Id
            VALUES (@UserId, @Token, @JwtId, @IsUsed, @IsRevoked, @CreatedAt, @ExpiresAt, @CreatedByIp)";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@UserId", token.UserId);
            cmd.Parameters.AddWithValue("@Token", token.Token);
            cmd.Parameters.AddWithValue("@JwtId", token.JwtId);
            cmd.Parameters.AddWithValue("@IsUsed", token.IsUsed);
            cmd.Parameters.AddWithValue("@IsRevoked", token.IsRevoked);
            cmd.Parameters.AddWithValue("@CreatedAt", token.CreatedAt);
            cmd.Parameters.AddWithValue("@ExpiresAt", token.ExpiresAt);
            cmd.Parameters.AddWithValue("@CreatedByIp", (object?)token.CreatedByIp ?? DBNull.Value);

            await conn.OpenAsync();
            var id = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(id);
        }

        public async Task<RefreshToken?> GetByTokenAsync(string tokenHash)
        {
            const string query = "SELECT * FROM RefreshTokens WHERE Token = @Token";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Token", tokenHash);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new RefreshToken
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
                    Token = reader.GetString(reader.GetOrdinal("Token")),
                    JwtId = reader.GetString(reader.GetOrdinal("JwtId")),
                    IsUsed = reader.GetBoolean(reader.GetOrdinal("IsUsed")),
                    IsRevoked = reader.GetBoolean(reader.GetOrdinal("IsRevoked")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    ExpiresAt = reader.GetDateTime(reader.GetOrdinal("ExpiresAt")),
                    CreatedByIp = reader.IsDBNull(reader.GetOrdinal("CreatedByIp")) ? null : reader.GetString(reader.GetOrdinal("CreatedByIp")),
                    RevokedByIp = reader.IsDBNull(reader.GetOrdinal("RevokedByIp")) ? null : reader.GetString(reader.GetOrdinal("RevokedByIp"))
                };
            }
            return null;
        }

        public async Task RevokeAsync(string tokenHash, string revokedByIp)
        {
            const string query = @"
            UPDATE RefreshTokens
            SET IsRevoked = 1, RevokedByIp = @RevokedByIp
            WHERE Token = @Token";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Token", tokenHash);
            cmd.Parameters.AddWithValue("@RevokedByIp", (object?)revokedByIp ?? DBNull.Value);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task MarkUsedAsync(string tokenHash)
        {
            const string query = "UPDATE RefreshTokens SET IsUsed = 1 WHERE Token = @Token";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Token", tokenHash);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RevokeAllForUserAsync(Guid userId)
        {
            const string query = @"
            UPDATE RefreshTokens
            SET IsRevoked = 1
            WHERE UserId = @UserId AND IsRevoked = 0";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
