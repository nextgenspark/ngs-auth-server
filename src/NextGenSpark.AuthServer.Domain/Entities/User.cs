using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextGenSpark.AuthServer.Domain.Entities
{
    public sealed class User
    {
        public Guid Id { get; init; }
        public Guid TenantId { get; init; }
        public string Username { get; init; } = string.Empty;
        public string PasswordHash { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public bool IsLocked { get; init; }
    }
}
