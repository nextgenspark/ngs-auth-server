using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextGenSpark.AuthServer.Domain.Entities
{
    public sealed class Tenant
    {
        public Guid Id { get; init; }
        public string TenantCode { get; init; } = string.Empty;
        public bool IsActive { get; init; }
    }
}
