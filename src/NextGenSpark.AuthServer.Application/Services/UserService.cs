using NextGenSpark.AuthServer.Application.Interfaces;
using NextGenSpark.AuthServer.Infrastructure.Interfaces;
using NextGenSpark.AuthServer.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextGenSpark.AuthServer.Application.Services
{
    public sealed class UserService : IUserService
    {
        private const int MAX_FAILED_ATTEMPTS = 5;
        private static readonly TimeSpan LOCK_WINDOW =
            TimeSpan.FromMinutes(10);

        private readonly UserRepository _userRepo;
        private readonly LoginAttemptRepository _attemptRepo;
        private readonly IPasswordHasher _hasher;

        public UserService(
            UserRepository userRepo,
            LoginAttemptRepository attemptRepo,
            IPasswordHasher hasher)
        {
            _userRepo = userRepo;
            _attemptRepo = attemptRepo;
            _hasher = hasher;
        }

        public async Task<bool> ValidateAsync(
            Guid tenantId,
            string username,
            string password,
            string ipAddress)
        {
            var user = await _userRepo.GetActiveAsync(tenantId, username);

            // User not found / inactive / locked
            if (user is null || user.IsLocked || !user.IsActive)
            {
                await _attemptRepo.RecordAsync(
                    tenantId, username, false, ipAddress);
                return false;
            }

            var success = _hasher.Verify(password, user.PasswordHash);

            // Record attempt
            await _attemptRepo.RecordAsync(
                tenantId, username, success, ipAddress);

            if (!success)
            {
                var failedCount =
                    await _attemptRepo.GetFailedCountAsync(
                        tenantId, username, LOCK_WINDOW);

                if (failedCount >= MAX_FAILED_ATTEMPTS)
                {
                    await _userRepo.LockAsync(user.Id);
                }
            }

            return success;
        }
    }

}
