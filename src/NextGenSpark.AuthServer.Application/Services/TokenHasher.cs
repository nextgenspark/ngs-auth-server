using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NextGenSpark.AuthServer.Application.Services
{
    public static class TokenHasher
    {
        public static string Hash(string token)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(
                sha.ComputeHash(Encoding.UTF8.GetBytes(token)));
        }
    }
}
