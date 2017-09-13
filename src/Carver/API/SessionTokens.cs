using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Carver.Data;
using Carver.Data.Models;
using Carver.Data.UserStore;
using Carver.Data.TokenStore;

namespace Carver.API
{
    internal class SessionTokens
    {
        private static readonly Lazy<SessionTokens> lazySingleton = new Lazy<SessionTokens>(() => new SessionTokens());

        private static SessionTokens Instance => lazySingleton.Value;

        private readonly ITokenStore<UserPermissions> TokenCache = DataStoreFactory.SessionTokenDataStore;
        private readonly IUserStore UserStore = DataStoreFactory.UserDataStore;

        static SessionTokens() { }

        public async Task<ClaimsPrincipal> GetUserClaimsFromSessionToken(string sessionToken)
        {
            if (sessionToken == null)
                return null;

            var userPermissions = await TokenCache.Lookup(sessionToken);

            if (userPermissions == null)
                return null;

            var identity = new GenericIdentity($"{userPermissions.UserId}");
            var claims = userPermissions.Permissions.Select(permission => Enum.GetName(typeof(Permission), permission)).ToArray();

            return new ClaimsPrincipal(new GenericPrincipal(identity, claims));
        }

        public async Task<string> ValidateUser(string username, string password)
        {
            if (!await UserStore.ValidateUser(username, password))
                return null;

            var user = await UserStore.GetUser(username);
            var permissions = await UserStore.GetPermissionsForUser(user.Value.Id);

            return await TokenCache.Create(permissions, null);
        }

        public async void InvalidateToken(string sessionToken)
        {
            await TokenCache.Invalidate(sessionToken);
        }
    }
}