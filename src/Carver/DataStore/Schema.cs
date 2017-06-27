using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;

namespace Carver
{
    public static class Schema
    {
        internal static Dictionary<string, List<string>> MigrationPath => new Dictionary<string, List<string>>
        {
            { "v0", new List<string>{ sql_v0_migrations_create }},
            { "v1", new List<string>{ sql_v1_user_groups_create, sql_v1_users_create, sql_v1_tokens_create }}
        };

        private const string sql_v0_migrations_create = "CREATE TABLE IF NOT EXISTS migrations ( name TEXT, " +
                                                        "execution_date TIMESTAMP, PRIMARY KEY(name) );";

        private const string sql_v1_user_groups_create = "CREATE TABLE IF NOT EXISTS user_groups ( id SERIAL, " +
                                                       "role_description TEXT UNIQUE, PRIMARY KEY(id) );";

        private const string sql_v1_users_create = "CREATE TABLE IF NOT EXISTS 'users' + " +
            @"(
                id INTEGER PRIMARY KEY,
                username TEXT NOT NULL, 
                email TEXT NOT NULL,
                hashed_password TEXT NOT NULL, 
                salt TEXT NOT NULL,
                hashing_iterations INTEGER, 
                group INTEGER,
                active INTEGER,
                created_on_date TEXT, 
                FOREIGN KEY(group) REFERENCES groups(id)
              );";

        private const string sql_v1_tokens_create = "CREATE TABLE IF NOT EXISTS 'tokens' + " +
            @"(
                id TEXT PRIMARY KEY,
                description TEXT NOT NULL, 
                expiration TEXT NOT NULL,
                enabled INTEGER NOT NULL, 
                created_on_date TEXT, 
              );";

        public static void BuildDatabaseSchema(IDbConnection connection)
        {
            connection.Execute(sql_v0_migrations_create);  // Create the migrations table if it does not already exist.

            var executedMigrations = connection.Query<string>(@"SELECT name FROM migrations");

            foreach (var unExecutedMigration in MigrationPath.Where(x => !executedMigrations.Contains(x.Key)))
            {
                foreach (var command in unExecutedMigration.Value)
                {
                    connection.Execute(command);
                }

                connection.Execute(@"INSERT INTO migrations (name, execution_date) VALUES (@Name, CURRENT_TIMESTAMP)",
                    new { Name = unExecutedMigration.Key });
            }
        }
    }
}
