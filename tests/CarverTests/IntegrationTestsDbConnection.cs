using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Carver.Data;
using Npgsql;
using Dapper;

namespace CarverTests
{
    public class IntegrationTestsDbConnection
    {
        public IDbConnection DbConnection { get; }
        
        public IntegrationTestsDbConnection()
        {
            DbConnection = new NpgsqlConnection(BuildConnectionString());

            ClearTables(DbConnection, GetTableNames(DbConnection));

            Schema.BuildDatabaseSchema(DbConnection);
        }

        private IEnumerable<string> GetTableNames(IDbConnection dbConnection)
        {
            const string sql = "SELECT table_name FROM information_schema.tables WHERE table_schema='public' AND table_type='BASE TABLE';";
            return dbConnection.Query<string>(sql);
        }

        private void ClearTables(IDbConnection dbConnection, IEnumerable<string> tableNames)
        {
            foreach (var table in tableNames)
            {
                dbConnection.Execute($"DROP TABLE IF EXISTS {table};");
            }
        }

        private static string BuildConnectionString()
        {
            var connectionOptions = new Dictionary<string, object>
            {
                { "Server", "127.0.0.1" },
                { "Port", 5432 },
                { "Database", "carver_test_db" },
                { "SslMode", "Disable"},
                { "UserId", "postgres"}
            };

            return string.Join("", connectionOptions.Select(x => string.Format("{0}={1};", x.Key, x.Value)));
        }
    }
}