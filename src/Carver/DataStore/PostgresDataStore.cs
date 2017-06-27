using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Carver.DataStore;
using Carver.Users;
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

        #region IUserStore Implementation

        public async Task<long> CreateUser(User user)
        {
            if (await GetUserId(user.Username) != null)
                throw new UserStoreException($"The username [{user.Username}] already exists.");

            return await Task.Factory.StartNew<long>(() =>
            {
                return DbConnection.QueryFirst<long>(@"INSERT INTO users (id, username, email, hashed_password, salt, hashing_iterations, user_group, active, created_on_date) " +
                                                     "VALUES (DEFAULT, @Username, @Email, @HashedPassword, @Salt, @HashingIterations, @Group, @Active, NOW()) " +
                                                     "RETURNING id;",
                    new
                    {
                        Username = user.Username,
                        Email = user.Email,
                        HashedPassword = user.HashedPassword,
                        Salt = user.Salt,
                        HashingIterations = user.HashingIterations,
                        Group = (int)(user.UserGroup),
                        Active = user.Active ? 1 : 0,
                        CreatedOnDate = user.CreationDate,
                    });
            });
        }
        
        public async Task<long> UpdateUser(long userId, User user)
        {
            User? storedUser = await GetUser(userId);

            if (storedUser == null)
                throw new UserStoreException($"Specified user id [{userId}] does not already exist.");

            return await Task.Factory.StartNew<long>(() =>
            {
                return DbConnection.QueryFirst<long>(@"UPDATE users SET username = @Username, email = @Email, hashed_password = @HashedPassword, " +
                                                 "salt = @Salt, hashing_iterations = @HashingIterations, user_group = @Group, active = @Active " +
                                                 "WHERE id= @ID RETURNING id;",
                new
                {
                    ID = userId,
                    Username = user.Username,
                    Email = user.Email,
                    HashedPassword = user.HashedPassword,
                    Salt = user.Salt,
                    HashingIterations = user.HashingIterations,
                    Group = (int)(user.UserGroup),
                    Active = user.Active ? 1 : 0,
                    CreatedOnDate = user.CreationDate,
                });
            });
        }

        public async Task InvalidateUser(long userId)
        {
            await Task.Factory.StartNew(() =>
            {
                DbConnection.Execute("UPDATE 'users' SET active=0 WHERE id=@ID", new { ID = userId });
            });
        }

        public async Task<User?> GetUser(long userId)
        {
            return await GetUser("SELECT * FROM users WHERE id=@ID", new { ID = userId });
        }

        public async Task<User?> GetUser(string username)
        {
            return await GetUser("SELECT * FROM users WHERE username=@Username", new { Username = username });
        }

        private async Task<User?> GetUser(string sql, object param)
        {
            return await Task.Factory.StartNew<User?>(() =>
            {
                var user = DbConnection.Query<dynamic>(sql, param)
                                       .DefaultIfEmpty(null)
                                       .FirstOrDefault();

                if (user == null)
                    return null;

                return new User(user.username, user.email, user.hashed_password, user.salt, user.hashing_iterations,
                    (UserGroup)(int)(user.user_group), DateTime.Parse(user.created_on_date), user.active != 0);
            });
        }

        public async Task<long?> GetUserId(string userName)
        {
            return await Task.Factory.StartNew<long?>(() =>
            {
                return DbConnection.Query<long?>("SELECT id FROM users WHERE username=@Username", new { Username = userName })
                               .DefaultIfEmpty(null)
                               .FirstOrDefault();
            });
        }

        #endregion

        #region ITokenStore Implementation

        public async Task<string> CreateNewToken(string description, DateTime? expiration)
        {
            return await Task.Factory.StartNew<string>(() =>
            {
                return DbConnection.QueryFirst<string>("INSERT INTO tokens (id, description, expiration, revoked, created_on_date)" +
                                                       " VALUES (@ID, @Description, @Expiration, @Revoked, @CreatedOnDate) RETURNING id;",
                    new { ID = Guid.NewGuid(), Description = description, Expiration = expiration, Revoked = 0, CreatedOnDate = DateTime.Now });
            });
        }

        public async Task InvalidateToken(string token)
        {
            if (!(await ValidTokenExists(token)))
                throw new TokenStoreException($"The specified token for revocation [{token}] does not exist");

            await Task.Factory.StartNew(() =>
            {
                DbConnection.Execute("UPDATE tokens SET revoked = 1 WHERE id=@Token", new { Token = token });
            });
        }

        public async Task<bool> ValidTokenExists(string token)
        {
            return await Task.Factory.StartNew<bool>(() =>
            {
                return DbConnection.Query("SELECT * FROM tokens WHERE id=@ID AND revoked=0", new { ID = token }).Any();
            });
        }

        #endregion
    }
}