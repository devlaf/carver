using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Carver.DataStore;
using Carver.DataStore.DataTypes;
using Npgsql;
using log4net;
using Dapper;

namespace Carver
{
    /// <summary>
    ///  A database abstraction layer, containing methods for simple CRUD operations 
    ///  pertaining to the user/token tables.
    /// </summary>
    internal class PostgresDataStore : IUserStore, ITokenStore
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PostgresDataStore));

        private IDbConnection DbConnection => new PostgresDBConnection().Connection;

        /// <summary>
        /// Constructor: Builds DB and handles migration
        /// </summary>
        /// <exception cref="DatabaseConnectionException">Could not establish an initial connection with the remote DB.</exception>
        public PostgresDataStore()
        {
            PostgresDBConnection.TestConnection(DbConnection);

            Schema.BuildDatabaseSchema(DbConnection);
        }

        public long CreateUser(User user)
        {
            throw new NotImplementedException();
        }

        public long UpdateUser(long userId, User user)
        {
            throw new NotImplementedException();
        }

        public void InvalidateUser(long userId)
        {
            throw new NotImplementedException();
        }

        public long GetUserId(string userName)
        {
            throw new NotImplementedException();
        }

        public string CreateNewToken(string description, DateTime? expiration)
        {
            throw new NotImplementedException();
        }

        public void InvalidateToken(string token)
        {
            throw new NotImplementedException();
        }

        public bool TokenExists(string token)
        {
            throw new NotImplementedException();
        }
    }
}
