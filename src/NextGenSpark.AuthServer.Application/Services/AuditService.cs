using NextGenSpark.AuthServer.Application.Interfaces;
using NextGenSpark.AuthServer.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NextGenSpark.AuthServer.Application.Services
{
    public sealed class AuditService : IAuditService
    {
        private readonly AuditLogRepository _repo;

        public AuditService(AuditLogRepository repo)
        {
            _repo = repo;
        }

        public async Task LogAsync(
            Guid? tenantId,
            Guid? userId,
            string eventType,
            object? eventData = null)
        {
            var json = eventData is null
                ? null
                : JsonSerializer.Serialize(eventData);

            await _repo.InsertAsync(
                tenantId,
                userId,
                eventType,
                json);
        }
    }

}
