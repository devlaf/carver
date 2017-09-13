using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carver.Config;
using Carver.Data.Models;
using System.Reactive.Linq;

namespace Carver.Data.TokenStore
{
    public class SessionTokenStoreProxy : ITokenStore<UserPermissions>
    {
        private readonly int CacheExpiryMillis;
        private readonly TokenCache<UserPermissions> Cache;

        public SessionTokenStoreProxy()
        {
            CacheExpiryMillis = Configuration.GetValue<int>("session_token_expiry_ms", 18000000); // 5 hrs
            Cache = new TokenCache<UserPermissions>(CacheExpiryMillis);
        }

        public async Task<string> Create(UserPermissions data, DateTimeOffset? expiration)
        {
            return await Task.Factory.StartNew<string>(() => {
                var sessionToken = Guid.NewGuid().ToString();
                if (expiration.HasValue)
                    Cache.StoreToken(sessionToken, data, millisFromNow(expiration.Value));
                else
                    Cache.StoreToken(sessionToken, data);
                return sessionToken;
            });
        }

        public async Task Invalidate(string token)
        {
            await Task.Factory.StartNew(() => Cache.BustToken(token));
        }

        public async Task<bool> Exists(string token)
        {
            return await Task.Factory.StartNew<bool>(() => (Cache.LookupToken(token) != null));
        }

        public async Task<UserPermissions> Lookup(string token)
        {
            return await Task.Factory.StartNew<UserPermissions>(() => (Cache.LookupToken(token)));
        }

        public IObservable<String> GetExpired()
        {
            return new List<string>().ToObservable();
        }

        private int millisFromNow(DateTimeOffset dto)
        {
            long difference = dto.ToUnixTimeMilliseconds() - DateTimeOffset.Now.ToUnixTimeMilliseconds();
            return difference < 0 ? 0 : difference > int.MaxValue ? int.MaxValue : (int)difference;
        }
    }
}