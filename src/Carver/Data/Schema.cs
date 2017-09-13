using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Carver.Data.Models;
using Dapper;

namespace Carver.Data
{
    public static class Schema
    {
        public static void BuildDatabaseSchema(IDbConnection connection)
        {
            connection.Execute(sql_m0_migrations_create);

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

        internal static Dictionary<string, List<string>> MigrationPath => new Dictionary<string, List<string>>
        {
            { "m0", new List<string>{ sql_m0_migrations_create }},
            { "m1", new List<string>{ sql_m1_claims_create, sql_m1_claims_populate }},
            { "m2", new List<string>{ sql_m2_users_create, sql_m2_users_idx_email }},
            { "m3", new List<string>{ sql_m3_user_permissions_create, sql_m3_user_permissions_idx_user_id_permission_id }},
            { "m4", new List<string>{ sql_m4_tokens_create }},
            { "m5", new List<string>{ sql_m5_users_add_pw_reset }},
            { "m6", new List<string>{ sql_m6_user_permissions_col_deleted_at, sql_m6_user_permissions_idx_user_id_deleted_at }}
        };

        private const string sql_m0_migrations_create = 
            @"CREATE TABLE IF NOT EXISTS migrations ( 
                name TEXT,
                execution_date TIMESTAMP,
                PRIMARY KEY(name)
            );";

        private const string sql_m1_claims_create =
            @"CREATE TABLE IF NOT EXISTS permissions (
                id SERIAL,
                description TEXT UNIQUE,
                PRIMARY KEY(id)
            );";

        private static readonly string sql_m1_claims_populate = Enum.GetValues(typeof(Permission))
            .Cast<int>()
            .Select(x => $"INSERT INTO permissions (id, description) values('{x}', '{Enum.GetName(typeof(Permission), x)}'); ")
            .Aggregate(string.Empty, (y, x) => y + x);

        private const string sql_m2_users_create =
            @"CREATE TABLE IF NOT EXISTS users (
                id SERIAL PRIMARY KEY,
                username TEXT UNIQUE, 
                email TEXT NOT NULL,
                hashed_password TEXT NOT NULL, 
                salt TEXT NOT NULL,
                hashing_iterations INTEGER, 
                created_at BIGINT, 
                deactivated_at BIGINT
            );";
        
        private const string sql_m2_users_idx_email =
            @"CREATE INDEX idx_email ON users (email);";
        
        private const string sql_m3_user_permissions_create =
            @"CREATE TABLE IF NOT EXISTS user_permissions (
                id SERIAL PRIMARY KEY,
                user_id BIGINT,
                permission_id BIGINT
              );";
        
        private const string sql_m3_user_permissions_idx_user_id_permission_id =
            @"CREATE INDEX idx_user_id_permission_id ON user_permissions (user_id, permission_id);";

        private const string sql_m4_tokens_create = 
            @"CREATE TABLE IF NOT EXISTS tokens (
                id TEXT PRIMARY KEY,
                description TEXT,
                created_at BIGINT,
                expires_at BIGINT,
                revoked_at BIGINT
              );";

        private const string sql_m5_users_add_pw_reset =
            @"ALTER TABLE users ADD COLUMN password_reset_token TEXT NULL;";
        
        private const string sql_m6_user_permissions_col_deleted_at =
            @"ALTER TABLE user_permissions ADD COLUMN deleted_at BIGINT NULL;";

        private const string sql_m6_user_permissions_idx_user_id_deleted_at =
            @"CREATE INDEX idx_user_id_deleted_at ON user_permissions (user_id, deleted_at);";
    }
}