using System;
using System.Data;
using System.Threading.Tasks;
using Carver.Data.Models;
using Carver.Data.DAOs;
using Xunit;

namespace CarverTests.DAOTests
{
    public class UserDAOTests
    {
        private IDbConnection DbConnection;

        public UserDAOTests()
        {
            DbConnection = new IntegrationTestsDbConnection().DbConnection;
        }

        [Fact]
        public async Task Create_Should_Succeed()
        {
            string userName = "MrCoffee";
            string email = "mrcoffee@folgers.com";
            string hashedPassword = "hashyHashbrowns";
            string salt = "orPepperMaybe";
            int hashingIteraitons = 4;
            long createdAt = 0L;
            
            var id = await UserDAO.Create(DbConnection, userName, email, hashedPassword, salt, hashingIteraitons, createdAt);
            User? user = await UserDAO.GetById(DbConnection, id);

            Assert.Equal(user?.Id, id);
            Assert.Equal(user?.Username, userName);
            Assert.Equal(user?.Email, email);
            Assert.Equal(user?.HashedPassword, hashedPassword);
            Assert.Equal(user?.Salt, salt);
            Assert.Equal(user?.HashingIterations, hashingIteraitons);
            Assert.Equal(user?.CreatedAt, createdAt);
        }

        [Fact]
        public async Task UpdateEmail_ForValidUser_Should_Succeed()
        {
            User user = await CreateRandomUser(DbConnection);
            Assert.NotNull(user.Email);

            string newEmail = "this_is_my_new_email_now@gmail.com";
            await UserDAO.UpdateEmail(DbConnection, user.Id, newEmail);
            
            var updatedUser = await UserDAO.GetById(DbConnection, user.Id);
            Assert.Equal(newEmail, updatedUser?.Email);
        }

        [Fact]
        public async Task UpdateEmail_ForInValidUser_Should_Throw()
        {
            string newEmail = "this_is_my_new_email_now@gmail.com";
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await UserDAO.UpdateEmail(DbConnection, -1, newEmail));
        }

        [Fact]
        public async Task UpdatePassword_ForValidUser_Should_Succeed()
        {
            User user = await CreateRandomUser(DbConnection);
            string resetToken = await UserDAO.GeneratePasswordResetToken(DbConnection, user.Id);

            string newPasswordHash = "thisIsMyNewPasswordHash";
            string newSalt = "orPepper?";
            int newHashingIteraitons = 42;
            long userId = await UserDAO.UpdatePassword(DbConnection, user.Id, resetToken, newPasswordHash, newSalt, newHashingIteraitons);

            var updatedUser = await UserDAO.GetById(DbConnection, userId);
            Assert.Equal(updatedUser?.Id, userId);
            Assert.Equal(updatedUser?.HashedPassword, newPasswordHash);
            Assert.Equal(updatedUser?.Salt, newSalt);
            Assert.Equal(updatedUser?.HashingIterations, newHashingIteraitons);

        }

        [Fact]
        public async Task UpdatePassword_ForInValidUser_Should_Throw()
        {
            string newPasswordHash = "thisIsMyNewPasswordHash";
            string newSalt = "orPepper?";
            int newHashingIteraitons = 42;
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                 await UserDAO.UpdatePassword(DbConnection, 1, "token", newPasswordHash, newSalt, newHashingIteraitons));
        }

        [Fact]
        public async Task UpdatePassword_ForInValidResetToken_Should_Throw()
        {
            User user = await CreateRandomUser(DbConnection);

            string newPasswordHash = "thisIsMyNewPasswordHash";
            string newSalt = "orPepper?";
            int newHashingIteraitons = 42;
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                 await UserDAO.UpdatePassword(DbConnection, 1, "invalidToken", newPasswordHash, newSalt, newHashingIteraitons));

            string resetToken = await UserDAO.GeneratePasswordResetToken(DbConnection, user.Id);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                 await UserDAO.UpdatePassword(DbConnection, 1, "stillInvalid", newPasswordHash, newSalt, newHashingIteraitons));
        }
        
        [Fact]
        public async Task Deactivate_Should_Succeed()
        {
            User user = await CreateRandomUser(DbConnection);
            Assert.Null(user.DeactivatedAt);

            await UserDAO.Deactivate(DbConnection, user.Id);

            User? updatedUser = await UserDAO.GetById(DbConnection, user.Id);
            Assert.NotNull(updatedUser?.DeactivatedAt);
        }

        [Fact]
        public async Task Deactivate_ForInValidUser_Should_Throw()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await UserDAO.Deactivate(DbConnection, -1));
        }

        [Fact]
        public async Task GetById_Should_Succeed()
        {
            User user = await CreateRandomUser(DbConnection);
            var queriedUser = await UserDAO.GetById(DbConnection, user.Id);
            Assert.Equal(user.Username, queriedUser?.Username);
            Assert.Equal(user.CreatedAt, queriedUser?.CreatedAt);
            Assert.Equal(user.DeactivatedAt, queriedUser?.DeactivatedAt);
            Assert.Equal(user.Salt, queriedUser?.Salt);
            Assert.Equal(user.HashedPassword, queriedUser?.HashedPassword);
            Assert.Equal(user.HashingIterations, queriedUser?.HashingIterations);
        }

        [Fact]
        public async Task GetById_ForNonExistantUser_ShouldReturnEmtpy()
        {
            var queried = await UserDAO.GetById(DbConnection, -1);
            Assert.False(queried.HasValue);
        }

        [Fact]
        public async Task GetByUserName_Should_Succeed()
        {
            User user = await CreateRandomUser(DbConnection);
            var queriedUser = await UserDAO.GetByUserName(DbConnection, user.Username);
            Assert.Equal(user.Username, queriedUser?.Username);
            Assert.Equal(user.CreatedAt, queriedUser?.CreatedAt);
            Assert.Equal(user.DeactivatedAt, queriedUser?.DeactivatedAt);
            Assert.Equal(user.Salt, queriedUser?.Salt);
            Assert.Equal(user.HashedPassword, queriedUser?.HashedPassword);
            Assert.Equal(user.HashingIterations, queriedUser?.HashingIterations);
        }
        
        [Fact]
        public async Task GetByUserName_ForNonExistantUser_ShouldReturnEmtpy()
        {
            var queried = await UserDAO.GetByUserName(DbConnection, "nonExistantUsername");
            Assert.False(queried.HasValue);
        }

        internal static async Task<User> CreateRandomUser(IDbConnection dbConnection)
        {
            string username = Guid.NewGuid().ToString();
            string email = $"{username}@blah.com";
            string hashedPassword = Guid.NewGuid().ToString();;
            string salt = Guid.NewGuid().ToString();;
            int hashingIteraitons = new Random().Next(0, 1000);
            long createdAt = 0L;
            
            long id = await UserDAO.Create(dbConnection, username, email, hashedPassword, salt, hashingIteraitons, createdAt);
            var user = await UserDAO.GetById(dbConnection, id);
            return user.Value;
        }
    }
}