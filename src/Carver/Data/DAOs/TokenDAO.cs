using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carver.Data.Models;
using Dapper;

namespace Carver.Data.DAOs
{
    public class TokenDAO
    {
        public static async Task<string> Create(IDbConnection dbConnection, string description, DateTimeOffset? expiration)
        {
            var sql = "INSERT INTO tokens (id, description, created_at, expires_at) " +
            "VALUES (@ID, @Description, @CreatedAt, @ExpiresAt) RETURNING id;";

            var values = new { 
                ID = Guid.NewGuid(),
                Description = description,
                CreatedAt = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                ExpiresAt = expiration?.ToUnixTimeMilliseconds()
            };
            
            return await Task.Factory.StartNew<string>(() => dbConnection.QueryFirst<string>(sql, values));
        }

        public static async Task Invalidate(IDbConnection dbConnection, string token)
        {
            var sql = "UPDATE tokens SET revoked_at=@RevokedAt WHERE id=@Token;";

            var values = new { Token = token, RevokedAt = DateTimeOffset.Now.ToUnixTimeMilliseconds() };

            await Task.Factory.StartNew(() => dbConnection.Execute(sql, values));
        }

        public static async Task<Token> GetById(IDbConnection dbConnection, string id)
        {
            var sql = @"SELECT id as ID, description as Description, expires_at as ExpiresAt, created_at as CreatedAt,
                        revoked_at as RevokedAt FROM tokens WHERE id=@ID;";

            var values = new { ID = id };

            return await Task.Factory.StartNew<Token>(() => dbConnection.QueryFirstOrDefault<Token>(sql, values));
        }

        public static async Task<List<Token>> GetByIdBulk(IDbConnection dbConnection, List<string> ids)
        {
            var sql = @"SELECT id as ID, description as Description, expires_at as ExpiresAt, created_at as CreatedAt,
                        revoked_at as RevokedAt FROM tokens WHERE id in (@Ids);";

            var values = new { Ids = ids };

            return await Task.Factory.StartNew<List<Token>>(() => dbConnection.QueryFirstOrDefault<List<Token>>(sql, values));
        }

        public static async Task Delete(IDbConnection dbConnection, string id)
        {
            var sql = @"DELETE FROM tokens WHERE id = @Id;";

            var values = new { Id = id };

            await Task.Factory.StartNew(() => dbConnection.Execute(sql, values));
        }

        public static async Task<List<string>> GetExpired(IDbConnection dbConnection, string pageFloor, int limit)
        {
            var sql = @"SELECT id FROM tokens WHERE expires_at < @Now AND id > @PageFloor ORDER BY id LIMIT @Limit;";

            var values = new 
            { 
                Now = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                PageFloor = pageFloor,
                Limit = limit
            };

            return await Task.Factory.StartNew<List<string>>(() => dbConnection.Query<string>(sql, values).AsList());
        }
    }
}