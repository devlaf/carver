using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Carver.Config;
using log4net;
using Npgsql;
using Dapper;

namespace Carver.Data
{
    internal class PostgresDBConnection
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PostgresDBConnection));

        public IDbConnection Connection => new NpgsqlConnection(BuildConnectionString());

        private static string BuildConnectionString()
        {
            var connectionOptions = new Dictionary<string, object>
            {
                { "Server",                 Configuration.GetValue<string>("db_server",                  "127.0.0.1" )},
                { "Port",                   Configuration.GetValue<int>   ("db_port",                    5432        )},
                { "Database",               Configuration.GetValue<string>("db_name",                    "carver_db" )},
                { "UserId",                 Configuration.GetValue<string>("db_username",                ""          )},
                { "Password",               Configuration.GetValue<string>("db_password",                ""          )},
                { "CommandTimeout",         Configuration.GetValue<int>   ("db_command_timeout_sec",     20          )},
                { "SslMode",                Configuration.GetValue<string>("db_ssl_mode",                "Require"   )},
                { "Pooling",                Configuration.GetValue<bool>  ("db_pooling",                 true        )},
                { "MinPoolSize",            Configuration.GetValue<int>   ("db_min_pool_size",           1           )},
                { "MaxPoolSize",            Configuration.GetValue<int>   ("db_max_pool_size",           20          )},
                { "ConnectionIdleLifeTime", Configuration.GetValue<int>   ("db_connection_lifetime_sec", 15          )}
            };

            return string.Join("", connectionOptions.Select(x => string.Format("{0}={1};", x.Key, x.Value)));
        }

        public static void TestConnection(IDbConnection con)
        {
            try
            {
                con.Execute("SELECT 1");
            }
            catch (NpgsqlException ex)
            {
                const string errorMessage = "Could not establish a connection to the Postgres database.  Check " +
                                            "the connection settings.";
                Log.ErrorFormat("{0}: {1}{2}", errorMessage, Environment.NewLine, ex);
                throw new DatabaseConnectionException(errorMessage, ex);
            }
        }
    }
}