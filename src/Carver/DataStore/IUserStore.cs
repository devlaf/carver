using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Carver.Users;

namespace Carver.DataStore
{
    internal interface IUserStore
    {
        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <returns>The db ID associated with the new user.</returns>
        /// <exception cref="UserStoreException">The specified username already exists.</exception>
        Task<long> CreateUser(User user);

        /// <summary>
        /// Overwrites data for an existing user.
        /// </summary>
        /// <returns>The db ID associated with the user.</returns>
        /// <exception cref="UserStoreException">The specified user id does not exist.</exception>
        Task<long> UpdateUser(long userId, User user);

        /// <summary>
        /// Marks a user as not-active, which indicates that there permissions are no longer valid.
        /// </summary>
        Task InvalidateUser(long userId);

        /// <summary>
        /// Gets the user id associated with a username.
        /// </summary>
        /// <returns>The user id associated with the specified username if it exists.  If not, null.</returns>
        Task<long?> GetUserId(string userName);

        /// <summary>
        /// Get details for a user if the user exists.  If not, null.
        /// </summary>
        Task<User?> GetUser(long userId);

        /// <summary>
        /// Get details for a user if the user exists.  If not, null.
        /// </summary>
        Task<User?> GetUser(string username);
    }
}