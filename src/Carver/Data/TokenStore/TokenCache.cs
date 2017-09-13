using System;
using ServiceStack.Redis;

namespace Carver.Data.TokenStore
{
    public class TokenCache<T>
    {
        private readonly int CacheExpiryMillis;
        private readonly RedisManagerPool ClientPool;
        
        public TokenCache(int defaultExpiryMillis) 
        {
            ClientPool = RedisConnection.ClientPool;
            CacheExpiryMillis = defaultExpiryMillis;
        }

        public T LookupToken(string token)
        {
            using (var client = ClientPool.GetClient())
            {
                return client.Get<T>(token);
            }
        }

        public void StoreToken(string token, T value, int expiresInMillis)
        {
            using (var client = ClientPool.GetClient())
            {
                client.Set(token, token, new TimeSpan(0, 0, 0, 0, expiresInMillis));
            }
        }

        public void StoreToken(string token, T value)
        {
            StoreToken(token, value, CacheExpiryMillis);
        }

        public void BustToken(string token) 
        {
            using (var client = ClientPool.GetClient())
            {
                client.Remove(token);
            }
        }
    }
}