using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Carver.Data.DAOs;
using Carver.Data.Models;
using log4net;
using Dapper;

namespace Carver.Data.UserStore
{
    public class UserStore : IUserStore
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UserStore));

        private readonly IDbConnection DbConnection;

        public UserStore(IDbConnection dbConnection)
        {
            DbConnection = dbConnection;
        }

        public async Task<long> CreateUser(string username, string password, string email)
        {
            ValidateEmail(email);
            long createdAt = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var userIdentity = await UserAuthentication.GenerateUserCreds(username, password);

            Log.Info($"Creating new user [username={username} email={email}]");
            return await UserDAO.Create(DbConnection, username, email, userIdentity.HashedPassword,
                userIdentity.Salt, userIdentity.HashingIterations, createdAt);
        }

        public async Task<long> UpdateEmail(long userId, string email)
        {
            ValidateEmail(email);
            Log.Info($"Updating user email for user {userId} to [{email}]");
            return await UserDAO.UpdateEmail(DbConnection, userId, email);
        }

        public async Task UpdateUserPassword(string username, string oldPassword, string newPassword)
        {
            if (!await ValidateUser(username, oldPassword))
                throw new ArgumentException("Specified username/oldPassword is not correct.");

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var user = (await UserDAO.GetByUserName(DbConnection, username)).Value;
                string resetToken = await UserDAO.GeneratePasswordResetToken(DbConnection, user.Id);

                var newUserIdentity = await UserAuthentication.GenerateUserCreds(username, newPassword);
                
                Log.Info(string.Format("Updating password for user " + username));
                await UserDAO.UpdatePassword(DbConnection, user.Id, resetToken, newUserIdentity.HashedPassword,
                    newUserIdentity.Salt, newUserIdentity.HashingIterations );
                
                transactionScope.Complete();
            }
        }
        
        public async Task DeactivateUser(long userId)
        {
            Log.Info($"Deactivating user {userId}");
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await UserDAO.Deactivate(DbConnection, userId);
                await UserPermissionsDAO.DeleteByUser(DbConnection, userId);
                transactionScope.Complete();
            }
        }

        public async Task<User?> GetUser(long userId)
        {
            return await UserDAO.GetById(DbConnection, userId);
        }

        public async Task<User?> GetUser(string username)
        {
            return await UserDAO.GetByUserName(DbConnection, username);
        }
        
        public async Task<UserPermissions> GetPermissionsForUser(long userId)
        {
            var permissions = await UserPermissionsDAO.GetByUserId(DbConnection, userId);
            return new UserPermissions(userId, permissions.Select(userPermission => userPermission.Permission).ToList());
        }

        public async Task EnsurePermissionsForUser(long userId, HashSet<Permission> permissions)
        {
            Log.Info($"Insuring user {userId} for permissions: [{permissions}]");
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var currentPermissions = await UserPermissionsDAO.GetByUserId(DbConnection, userId);
                var toAdd = permissions.Except(currentPermissions.Select(x => x.Permission));
                foreach (var permission in toAdd)
                {
                    await UserPermissionsDAO.Create(DbConnection, userId, (int)permission);
                }
            }
        }

        public async Task RevokePermissionsForUser(long userId, HashSet<Permission> permissions)
        {
            Log.Info($"Remvoing permissions for user {userId} : [{permissions}]");
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var currentPermissions = await UserPermissionsDAO.GetByUserId(DbConnection, userId);
                var toRemove = currentPermissions.Select(x => x.Permission).Intersect(permissions);
                foreach (var permission in toRemove)
                {
                    await UserPermissionsDAO.Create(DbConnection, userId, (int)permission);
                }
                transactionScope.Complete();
            }
        }

        public async Task<bool> ValidateUser(string username, string password)
        {
            var user = await GetUser(username);

            if (user == null)
                return false;

            if (user.Value.DeactivatedAt.HasValue)
                return false;

            var userCreds = new UserAuthentication.UserCredentials(user.Value.HashedPassword, 
                user.Value.Salt, user.Value.HashingIterations);

            return await (UserAuthentication.ConfirmUserPassword(userCreds, password));
        }

        private static void ValidateEmail(string emailAddress)
        {
            if (!emailAddress.Contains("@"))  // Clearly I'm being very thorough.
                throw new InvalidEmailException("The provided email did not specify a domain.");
        }

        internal class InvalidEmailException : Exception
        {
            public InvalidEmailException(string message) 
                : base(message) { }
            public InvalidEmailException(string message, Exception innerException) 
                : base(message, innerException) { }
        }
    }
}