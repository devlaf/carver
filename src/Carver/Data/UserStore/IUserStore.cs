using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carver.Data.Models;

namespace Carver.Data.UserStore
{
    internal interface IUserStore
    {
        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <returns>The ID associated with the new user.</returns>
        /// <exception cref="InvalidOperationException">The specified username already exists.</exception>
        /// <exception cref="UserStore.InvalidEmailException">The specified email is not valid.</exception>
        Task<long> CreateUser(string username, string password, string email);

        /// <summary>
        /// Updates email for an existing user.
        /// </summary>
        /// <returns>The ID associated with the user.</returns>
        /// <exception cref="InvalidOperationException">The specified user id does not exist.</exception>
        /// <exception cref="UserStore.InvalidEmailException">The specified email is not valid.</exception>
        Task<long> UpdateEmail(long userId, string email);

        /// <summary>
        /// Updates password for an existing user.
        /// </summary>
        /// <exception cref="ArgumentException">The specified username does not exist, user is deactivated, or the oldPassword is not correct.</exception>
        Task UpdateUserPassword(string username, string oldPassword, string newPassword);

        /// <summary>
        /// Marks a user as inactive, and removes their permissions.
        /// </summary>
        /// <exception cref="InvalidOperationException">The specified user id does not exist.</exception>
        Task DeactivateUser(long userId);

        /// <summary>
        /// Get details for a user if the user exists.  If not, null.
        /// </summary>
        Task<User?> GetUser(long userId);

        /// <summary>
        /// Get details for a user if the user exists.  If not, null.
        /// </summary>
        Task<User?> GetUser(string username);

        /// <summary>
        /// Get all permissions associated with a user. If user does not exist, empty collection.
        /// </summary>
        Task<UserPermissions> GetPermissionsForUser(long userId);

        /// <summary>
        /// Grant a user the specified permissions.
        /// </summary>
        /// <exception cref="InvalidOperationException">The specified user id does not exist.</exception>
        Task EnsurePermissionsForUser(long userId, HashSet<Permission> permissions);

        /// <summary>
        /// Remove the specified permissions from a user where they exist.
        /// </summary>
        /// <exception cref="InvalidOperationException">The specified user id does not exist.</exception>
        Task RevokePermissionsForUser(long userId, HashSet<Permission> permissions);

        /// <summary>
        /// Verify that user exists and the password for that user is the right one.
        /// </summary>
        /// <returns>
        /// True if the user exists, has not been deactivated, and the password is correct. False otherwise.
        /// </returns>
        Task<bool> ValidateUser(string username, string password);
    }
}