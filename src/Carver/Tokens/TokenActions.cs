using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Carver.DataStore;
using Carver.Users;
using log4net;

namespace Carver.Tokens
{
    internal static class TokenActions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TokenActions));

        public static async Task<string> CreateNewToken(string description, DateTime? expiration = null)
        {
            ITokenStore tokenStore = new PostgresDataStore();

            var token = await tokenStore.CreateNewToken(description, expiration);

            Log.Info($"Created new token [{token}] with description [{description}] and expiration [{expiration}]");

            return token;
        }

        public static async Task InvalidateToken(string token)
        {
            ITokenStore tokenStore = new PostgresDataStore();

            await tokenStore.InvalidateToken(token);

            Log.Info($"Invalidated token [{token}].");
        }

        public static async Task<bool> ValidTokenExists(string token)
        {
            ITokenStore tokenStore = new PostgresDataStore();

            return await tokenStore.ValidTokenExists(token);
        }
    }
}
