using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Carver.DataStore;
using Carver.Users;

namespace Carver.API
{
    internal class SessionTokens
    {
        private static Dictionary<string, string> Tokens = new Dictionary<string, string>();

        #region Singleton

        private static readonly Lazy<SessionTokens> lazyConfig = new Lazy<SessionTokens>(() => new SessionTokens());

        private static SessionTokens Instance => lazyConfig.Value;

        /// <summary> Static constructor required so compiler does not tag with beforefieldinit </summary>
        static SessionTokens() { }

        #endregion

        public static async Task<ClaimsPrincipal> GetUserClaimsFromApiKey(string apiKey)
        {
            if (apiKey == null)
                return null;

            string username;
            if (!Tokens.TryGetValue(apiKey, out username))
                return null;

            var user = await UserActions.GetUser(DataStoreFactory.UserDataStore, username);

            return new ClaimsPrincipal(new GenericIdentity(user?.Username, Enum.GetName(typeof(UserGroup), user?.UserGroup)));
        }

        public static async Task<string> ValidateUser(string username, string password)
        {
            if (!await UserActions.ValidateUser(DataStoreFactory.UserDataStore, username, password))
                return null;

            var user = await UserActions.GetUser(DataStoreFactory.UserDataStore, username);
            var apiKey = Guid.NewGuid().ToString();
            Tokens.Add(apiKey, user?.Username);

            return apiKey;
        }

        public static void RemoveApiKey(string apiKey)
        {
            if (Tokens.ContainsKey(apiKey))
                Tokens.Remove(apiKey);
        }
    }
}
