using System;
using System.Data;
using System.Collections.Generic;
using Carver.Data.Models;
using Carver.Data.UserStore;
using Carver.Data.TokenStore;

namespace Carver.Data
{
    internal static class DataStoreFactory
    {
        private static readonly Lazy<IDbConnection> lazyDbConnection = new Lazy<IDbConnection>(() =>
            {
                var connection = new PostgresDBConnection().Connection;
                PostgresDBConnection.TestConnection(connection);
                Schema.BuildDatabaseSchema(connection);
                return connection;
            }
        );

        private static IDbConnection DbConnection => lazyDbConnection.Value;

        static DataStoreFactory() { }

        internal static ITokenStore<string> AppTokenDataStore => new CachedAppTokenStore(DbConnection);
        internal static ITokenStore<UserPermissions> SessionTokenDataStore => new SessionTokenStoreProxy();
        internal static IUserStore UserDataStore => new UserStore.UserStore(DbConnection);
    }
}