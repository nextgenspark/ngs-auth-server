using Dapper;
using NextGenSpark.AuthServer.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextGenSpark.AuthServer.OpenIddict.Stores
{
    public sealed class ClientStore
    {
        private readonly DbConnectionFactory _db;

        public ClientStore(DbConnectionFactory db)
            => _db = db;

        public async Task<ClientData?> GetClientAsync(
            Guid tenantId,
            string clientId)
        {
            const string sql = """
        SELECT c.id, c.client_id, c.client_type, c.redirect_uris
        FROM clients c
        WHERE c.tenant_id = @TenantId
          AND c.client_id = @ClientId
          AND c.is_active = true
        """;

            using var conn = _db.Create();
            return await conn.QuerySingleOrDefaultAsync<ClientData>(
                sql, new { TenantId = tenantId, ClientId = clientId });
        }
    }

    public sealed record ClientData(
        Guid Id,
        string ClientId,
        string ClientType,
        string? RedirectUris
    );
}
