using System;
using System.Data;
using System.Threading.Tasks;
using Carver.Config;

namespace Carver.Data.TokenStore
{
    public class CachedAppTokenStore : ITokenStore<string>
    {
        private readonly int CacheExpiryMillis;
        private readonly AppTokenStore TokenStore;
        private readonly TokenCache<string> Cache;

        public CachedAppTokenStore(IDbConnection dbConnection)
        {
            CacheExpiryMillis = Configuration.GetValue<int>("app_token_cache_expiry_ms", 60000);
            TokenStore = new AppTokenStore(dbConnection);
            Cache = new TokenCache<string>(CacheExpiryMillis);
        }

        public async Task<string> Create(string data, DateTimeOffset? expiration)
        {
            var token = await TokenStore.Create(data, expiration);
            Cache.StoreToken(token, token);
            return token;
        }

        public async Task Invalidate(string token)
        {
            await TokenStore.Invalidate(token);
            Cache.BustToken(token);
        }

        public async Task<bool> Exists(string token)
        {
            if (Cache.LookupToken(token) != null)
                return true;

            return await TokenStore.Exists(token);
        }

        public async Task<string> Lookup(string token)
        {
            var cached = Cache.LookupToken(token);
            return cached ?? await TokenStore.Lookup(token);
        }

        public IObservable<String> GetExpired()
        {
            return TokenStore.GetExpired();
        }
    }
}