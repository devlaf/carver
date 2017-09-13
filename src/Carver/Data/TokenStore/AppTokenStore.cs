using System;
using System.Data;
using System.Threading.Tasks;
using log4net;
using Carver.Data.DAOs;
using Carver.Data.Models;


namespace Carver.Data.TokenStore
{
    public class AppTokenStore : ITokenStore<string>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AppTokenStore));

        private readonly IDbConnection DbConnection;

        public AppTokenStore(IDbConnection dbConnection)
        {
            DbConnection = dbConnection;
        }

        public async Task<string> Create(string description, DateTimeOffset? expiration)
        {
            return await TokenDAO.Create(DbConnection, description, expiration);
        }

        public async Task Invalidate(string token)
        {
            await TokenDAO.Invalidate(DbConnection, token);
        }

        public async Task<bool> Exists(string token)
        {
            var found = await TokenDAO.GetById(DbConnection, token);

            if (found == null)
                return false;

            if (found.ExpiresAt != null && found.ExpiresAt < DateTimeOffset.Now.ToUnixTimeMilliseconds())
                return false;
            
            return true;
        }

        public async Task<string> Lookup(string token)
        {
            var found = await TokenDAO.GetById(DbConnection, token);

            if (found == null)
                return string.Empty;

            if (found.ExpiresAt != null && found.ExpiresAt < DateTimeOffset.Now.ToUnixTimeMilliseconds())
                return string.Empty;
            
            return found.ID;
        }

        public IObservable<string> GetExpired()
        {
            var pagingStream = AsyncEnumerable.ToObservable<string, string>(
                "0",
                page => page.Count == 0,
                page => page.Count > 0 ? page[page.Count - 1] : "0",
                async (floor) => await TokenDAO.GetExpired(DbConnection, floor, 500)
            );

            return pagingStream;
        }
    }
}