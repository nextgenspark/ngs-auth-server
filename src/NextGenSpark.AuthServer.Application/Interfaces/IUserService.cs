using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextGenSpark.AuthServer.Application.Interfaces
{
    public interface IUserService
    {
        Task<bool> ValidateAsync(
            Guid tenantId,
            string username,
            string password,
            string ipAddress);
    }
}
