using System;
using Carver.Config;
using ServiceStack.Redis;
using log4net;

namespace Carver.Data
{
    public static class RedisConnection
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RedisConnection));

        public static RedisManagerPool ClientPool => BuildClientPool();

        private static RedisManagerPool BuildClientPool()
        {
            string connectionStr = string.Format("redis://{0}:{1}@{2}:{3}?ssl={4}", 
                Configuration.GetValue<string>("redis_client_id",   "carver"    ),
                Configuration.GetValue<string>("redis_password",    ""          ),
                Configuration.GetValue<string>("redis_hostname",    "127.0.0.1" ),
                Configuration.GetValue<int>   ("redis_port",        6379        ),
                Configuration.GetValue<bool>  ("redis_use_ssl",     false       ));

            Log.Info($"Connecting to redis with connection str:[{connectionStr}]...");
            return new RedisManagerPool(connectionStr);
        }
    }
}