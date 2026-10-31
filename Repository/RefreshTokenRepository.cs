using JWTAuthenticationAPI.Models;
using Microsoft.Data.SqlClient;
using System.Data;

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
            INSERT INTO RefreshTokens (UserId, Token, IsRevoked, ExpiresAt)
            OUTPUT INSERTED.Id
            VALUES (@UserId, @Token, @IsRevoked, @ExpiresAt)";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = token.UserId;
            cmd.Parameters.Add("@Token", SqlDbType.NVarChar, 256).Value = token.Token;
            cmd.Parameters.Add("@IsRevoked", SqlDbType.Bit).Value = token.IsRevoked;
            cmd.Parameters.Add("@ExpiresAt", SqlDbType.DateTime).Value = token.ExpiresAt;

            await conn.OpenAsync();
            var id = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(id);
        }

        public async Task<RefreshToken?> GetByTokenAsync(string tokenHash)
        {
            const string query = @"
                SELECT Id, UserId, Token, IsRevoked, CreatedAt, ExpiresAt
                FROM RefreshTokens
                WHERE Token = @Token";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@Token", SqlDbType.NVarChar, 256).Value = tokenHash;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new RefreshToken
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    Token = reader.GetString(reader.GetOrdinal("Token")),
                    IsRevoked = reader.GetBoolean(reader.GetOrdinal("IsRevoked")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    ExpiresAt = reader.GetDateTime(reader.GetOrdinal("ExpiresAt")),
                };
            }
            return null;
        }

        public async Task RevokeAsync(string tokenHash)
        {
            const string query = @"
            UPDATE RefreshTokens
            SET IsRevoked = 1
            WHERE Token = @Token";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@Token", SqlDbType.NVarChar, 256).Value = tokenHash;

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();

            Console.WriteLine($"Rows affected: {rows}");
        }

        public async Task RevokeAllForUserAsync(int userId)
        {
            const string query = @"
            UPDATE RefreshTokens
            SET IsRevoked = 1
            WHERE UserId = @UserId AND IsRevoked = 0";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> RotateAsync(
    string oldTokenHash,
    RefreshToken newToken)
        {
            using var conn = new SqlConnection(_connectionString);

            await conn.OpenAsync();

            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                const string revokeQuery = @"
            UPDATE RefreshTokens
            SET IsRevoked = 1
            WHERE Token = @Token
              AND IsRevoked = 0
              AND ExpiresAt > SYSUTCDATETIME()";

                using var revokeCmd = new SqlCommand(
                    revokeQuery,
                    conn,
                    (SqlTransaction)transaction);

                revokeCmd.Parameters.Add("@Token", SqlDbType.NVarChar, 256)
                    .Value = oldTokenHash;

                var rows = await revokeCmd.ExecuteNonQueryAsync();

                if (rows != 1)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                const string insertQuery = @"
            INSERT INTO RefreshTokens
                (UserId, Token, IsRevoked, ExpiresAt)
            VALUES
                (@UserId, @Token, @IsRevoked, @ExpiresAt)";

                using var insertCmd = new SqlCommand(
                    insertQuery,
                    conn,
                    (SqlTransaction)transaction);

                insertCmd.Parameters.Add("@UserId", SqlDbType.Int)
                    .Value = newToken.UserId;

                insertCmd.Parameters.Add("@Token", SqlDbType.NVarChar, 256)
                    .Value = newToken.Token;

                insertCmd.Parameters.Add("@IsRevoked", SqlDbType.Bit)
                    .Value = newToken.IsRevoked;

                insertCmd.Parameters.Add("@ExpiresAt", SqlDbType.DateTime)
                    .Value = newToken.ExpiresAt;

                await insertCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
