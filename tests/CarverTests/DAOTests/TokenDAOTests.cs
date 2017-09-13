using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carver.Data.DAOs;
using Xunit;

namespace CarverTests.DAOTests
{
    public class TokenDAOTests
    {
        private IDbConnection DbConnection;

        public TokenDAOTests()
        {
            DbConnection = new IntegrationTestsDbConnection().DbConnection;
        }

        [Fact]
        public async Task CreateToken_WithNullExpiration_Should_Succeed()
        {
            var token = await TokenDAO.Create(DbConnection, "description", null);
            Assert.NotNull(token);

            var fullToken = await TokenDAO.GetById(DbConnection, token);
            Assert.Equal(token, fullToken.ID);
            Assert.NotNull(fullToken.CreatedAt);
            Assert.Null(fullToken.ExpiresAt);
            Assert.Null(fullToken.RevokedAt);
        }

        [Fact]
        public async Task CreateToken_WithExpiration_Should_Succeed()
        {
            DateTimeOffset now = DateTimeOffset.Now;
            var token = await TokenDAO.Create(DbConnection, "description", now);
            Assert.NotNull(token);

            var fullToken = await TokenDAO.GetById(DbConnection, token);
            Assert.Equal(token, fullToken.ID);
            Assert.Equal(now.ToUnixTimeMilliseconds(), fullToken.ExpiresAt);
        }
        
        [Fact]
        public async Task InvalidateToken_Should_Succeed()
        {
            var token = await TokenDAO.Create(DbConnection, "description", null);
            Assert.NotNull(token);

            var fullToken = await TokenDAO.GetById(DbConnection, token);
            Assert.Null(fullToken.ExpiresAt);

            await TokenDAO.Invalidate(DbConnection, token);

            fullToken = await TokenDAO.GetById(DbConnection, token);
            Assert.NotNull(fullToken.RevokedAt);
        }

        [Fact]
        public async Task GetExpired_Should_Succeed()
        {
            var expiredTokenA = await TokenDAO.Create(DbConnection, "description", DateTimeOffset.MinValue);
            var expiredTokenB = await TokenDAO.Create(DbConnection, "description", DateTimeOffset.MinValue);
            var expiredTokenC = await TokenDAO.Create(DbConnection, "description", DateTimeOffset.MinValue);
            var expiredTokenD = await TokenDAO.Create(DbConnection, "description", DateTimeOffset.MinValue);
            var notExpiredTokenA = await TokenDAO.Create(DbConnection, "description", DateTimeOffset.MaxValue);
            var notExpiredTokenB = await TokenDAO.Create(DbConnection, "description", DateTimeOffset.MaxValue);

            var expiredTokens = new List<string> { expiredTokenA, expiredTokenB, expiredTokenC, expiredTokenD };

            var expiredPage1 = await TokenDAO.GetExpired(DbConnection, "0", 3);
            Assert.Equal(expiredPage1.Count, 3);
            foreach (var expiredToken in expiredPage1)
            {
                Assert.Contains(expiredToken, expiredTokens);
            }

            var expiredPage2 = await TokenDAO.GetExpired(DbConnection, expiredPage1[2], 3);
            Assert.Equal(expiredPage2.Count, 1);
            Assert.Contains(expiredPage2[0], expiredTokens);
        }
    }
}