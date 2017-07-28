using System;
using System.Threading.Tasks;
using Carver.DataStore;
using log4net;

namespace Carver.Tokens
{
    internal static class TokenActions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TokenActions));

        public static async Task<string> CreateNewToken(ITokenStore tokenStore, string description, DateTime? expiration = null)
        {
            var token = await tokenStore.CreateNewToken(description, expiration);

            Log.Info($"Created new token [{token}] with description [{description}] and expiration [{expiration}]");

            return token;
        }

        public static async Task InvalidateToken(ITokenStore tokenStore, string token)
        {
            await tokenStore.InvalidateToken(token);

            Log.Info($"Invalidated token [{token}].");
        }

        public static async Task<bool> ValidTokenExists(ITokenStore tokenStore, string token)
        {
            return await tokenStore.ValidTokenExists(token);
        }
    }
}
