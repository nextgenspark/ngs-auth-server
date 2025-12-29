using NextGenSpark.AuthServer.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace NextGenSpark.AuthServer.Application.Services
{
    public sealed class RefreshTokenService
    {
        private readonly RefreshTokenRepository _repo;

        public RefreshTokenService(RefreshTokenRepository repo)
        {
            _repo = repo;
        }

        public async Task<string> RotateAsync(
            Guid authorizationId,
            string oldRefreshToken)
        {
            var oldHash = TokenHasher.Hash(oldRefreshToken);

            // 🚨 Reuse detection
            if (await _repo.IsRevokedAsync(oldHash))
            {
                throw new SecurityException(
                    "Refresh token reuse detected");
            }

            // Revoke old token
            await _repo.RevokeAsync(oldHash);

            // Generate new refresh token
            var newToken = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64));

            var newHash = TokenHasher.Hash(newToken);

            await _repo.StoreAsync(
                authorizationId,
                newHash,
                DateTime.UtcNow.AddDays(7));

            return newToken;
        }
    }
}
