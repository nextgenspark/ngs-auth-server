using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextGenSpark.AuthServer.Infrastructure.Persistence
{
    public sealed class DbConnectionFactory
    {
        private readonly IConfiguration _config;

        public DbConnectionFactory(IConfiguration config)
            => _config = config;

        public NpgsqlConnection Create()
            => new(_config.GetConnectionString("AuthDb"));
    }
}
