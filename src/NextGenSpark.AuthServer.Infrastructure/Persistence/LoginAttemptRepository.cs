using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextGenSpark.AuthServer.Infrastructure.Persistence
{
    public sealed class LoginAttemptRepository
    {
        private readonly DbConnectionFactory _db;

        public LoginAttemptRepository(DbConnectionFactory db)
            => _db = db;

        public async Task RecordAsync(
            Guid tenantId,
            string username,
            bool isSuccess,
            string ipAddress)
        {
            const string sql = """
            INSERT INTO login_attempts
            (id, tenant_id, username, is_success, ip_address)
            VALUES (gen_random_uuid(), @TenantId, @Username, @IsSuccess, @Ip::inet)
            """;

            using var conn = _db.Create();
            await conn.ExecuteAsync(sql, new
            {
                TenantId = tenantId,
                Username = username,
                IsSuccess = isSuccess,
                Ip = ipAddress
            });
        }

        public async Task<int> GetFailedCountAsync(
            Guid tenantId,
            string username,
            TimeSpan window)
        {
            const string sql = """
            SELECT COUNT(*)
            FROM login_attempts
            WHERE tenant_id = @TenantId
              AND username = @Username
              AND is_success = false
              AND attempted_at >= now() - @Window
            """;

            using var conn = _db.Create();
            return await conn.ExecuteScalarAsync<int>(sql, new
            {
                TenantId = tenantId,
                Username = username,
                Window = window
            });
        }
    }
}
