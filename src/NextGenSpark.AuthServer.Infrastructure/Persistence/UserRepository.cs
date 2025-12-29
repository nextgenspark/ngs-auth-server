using Dapper;
using NextGenSpark.AuthServer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextGenSpark.AuthServer.Infrastructure.Persistence
{
    public sealed class UserRepository
    {
        private readonly DbConnectionFactory _db;

        public UserRepository(DbConnectionFactory db)
            => _db = db;

        public async Task<User?> GetActiveAsync(
            Guid tenantId,
            string username)
        {
            const string sql = """
        SELECT id, tenant_id, username, password_hash,
               is_active, is_locked
        FROM users
        WHERE tenant_id = @TenantId
          AND username = @Username
          AND is_active = true
        """;

            using var conn = _db.Create();
            return await conn.QuerySingleOrDefaultAsync<User>(
                sql, new { TenantId = tenantId, Username = username });
        }
        public async Task LockAsync(Guid userId)
        {
            const string sql = """
    UPDATE users
    SET is_locked = true,
        updated_at = now()
    WHERE id = @UserId
    """;

            using var conn = _db.Create();
            await conn.ExecuteAsync(sql, new { UserId = userId });
        }
    }
}
