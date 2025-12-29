using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextGenSpark.AuthServer.Application.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(
            Guid? tenantId,
            Guid? userId,
            string eventType,
            object? eventData = null);
    }
}
