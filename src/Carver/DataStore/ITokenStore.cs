using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Carver.DataStore
{
    public interface ITokenStore
    {
        /// <summary>
        /// Add a new auth token to the db
        /// </summary>
        /// <returns>The token string.</returns>
        Task<string> CreateNewToken(string description, DateTime? expiration);

        /// <summary>
        /// Markes the specified token as revoked, indicating that its permissions are no longer valid.
        /// </summary>
        Task InvalidateToken(string token);

        /// <summary>
        /// Checks to see if a token exists in the db and has not been revoked.
        /// </summary>
        Task<bool> ValidTokenExists(string token);
    }
}