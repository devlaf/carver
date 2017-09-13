using System;
using System.Data;
using System.Collections.Generic;
using Carver.Data;
using Carver.Data.DAOs;
using Carver.Data.TokenStore;
using System.Reactive.Linq;
using log4net;

namespace Carver.Jobs.CronJobs
{
    public class RemoveExpiredTokensJob : ICronJob
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RemoveExpiredTokensJob));
        private ITokenStore<string> TokenStore = DataStoreFactory.AppTokenDataStore;
        private IDbConnection connection = new PostgresDBConnection().Connection;

        private const int EXPIRATION_WINDOW_DAYS = 30;

        public string CronFormattedSchedule()
        {
            return "0 23 * * *";
        }

        public void Run()
        {
            var expiredIds = new List<string>(TokenStore.GetExpired().ToEnumerable());
            var expiredTokens = TokenDAO.GetByIdBulk(connection, expiredIds).Result;

            long expirationCutoff = new DateTimeOffset(DateTime.UtcNow, new TimeSpan(-EXPIRATION_WINDOW_DAYS, 0, 0)).ToUnixTimeMilliseconds();
            foreach (var expiredToken in expiredTokens)
            {
                if (expiredToken.ExpiresAt > expirationCutoff)
                    continue;
                
                Log.Info($"Removing expired token { expiredToken.ID }");
                TokenDAO.Delete(connection, expiredToken.ID).Wait();
            }
        }
    }
}