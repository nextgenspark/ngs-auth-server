using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextGenSpark.AuthServer.Infrastructure.Persistence
{
    public sealed class RefreshTokenRepository
    {
        private readonly DbConnectionFactory _db;

        public RefreshTokenRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public async Task<bool> IsRevokedAsync(string tokenHash)
        {
            const string sql = """
        SELECT 1 FROM refresh_token_blacklist
        WHERE token_hash = @Hash
        """;

            using var conn = _db.Create();
            return await conn.ExecuteScalarAsync<int?>(
                sql, new { Hash = tokenHash }) != null;
        }

        public async Task RevokeAsync(string tokenHash)
        {
            const string sql = """
        INSERT INTO refresh_token_blacklist(token_hash)
        VALUES (@Hash)
        ON CONFLICT DO NOTHING
        """;

            using var conn = _db.Create();
            await conn.ExecuteAsync(sql, new { Hash = tokenHash });
        }

        public async Task StoreAsync(
            Guid authorizationId,
            string tokenHash,
            DateTime expiresAt)
        {
            const string sql = """
        INSERT INTO tokens
        (id, authorization_id, token_type, token_hash, expires_at)
        VALUES (gen_random_uuid(), @AuthId, 'refresh', @Hash, @Expiry)
        """;

            using var conn = _db.Create();
            await conn.ExecuteAsync(sql, new
            {
                AuthId = authorizationId,
                Hash = tokenHash,
                Expiry = expiresAt
            });
        }
    }
}
