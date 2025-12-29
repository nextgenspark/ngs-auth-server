using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextGenSpark.AuthServer.Infrastructure.Persistence
{
    public sealed class AuditLogRepository
    {
        private readonly DbConnectionFactory _db;

        public AuditLogRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public async Task InsertAsync(
            Guid? tenantId,
            Guid? userId,
            string eventType,
            string? eventData)
        {
            const string sql = """
        INSERT INTO audit_logs
        (id, tenant_id, user_id, event_type, event_data)
        VALUES (gen_random_uuid(), @TenantId, @UserId, @EventType, @EventData::jsonb)
        """;

            using var conn = _db.Create();
            await conn.ExecuteAsync(sql, new
            {
                TenantId = tenantId,
                UserId = userId,
                EventType = eventType,
                EventData = eventData
            });
        }
    }
}
