using Isopoh.Cryptography.Argon2;
using NextGenSpark.AuthServer.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextGenSpark.AuthServer.Infrastructure.Security
{
    public sealed class Argon2PasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
            => Argon2.Hash(
                password,
                timeCost: 3,
                memoryCost: 65536,
                parallelism: Environment.ProcessorCount);

        public bool Verify(string password, string hash)
            => Argon2.Verify(hash, password);
    }
}
