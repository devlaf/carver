using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Carver.Data.DAOs;
using Carver.Data.Models;
using Xunit;

namespace CarverTests.DAOTests
{
    public class UserPermissionsDAOTests
    {
        private IDbConnection DbConnection;

        public UserPermissionsDAOTests()
        {
            DbConnection = new IntegrationTestsDbConnection().DbConnection;
        }

        [Fact]
        public async Task Create_UserPermission_ShouldSucceed()
        {
            var user = await UserDAOTests.CreateRandomUser(DbConnection);
            List<UserPermission> userPermsOriginal = await UserPermissionsDAO.GetByUserId(DbConnection, user.Id);
            Assert.Equal(userPermsOriginal.Count, 0);
            
            await UserPermissionsDAO.Create(DbConnection, user.Id, (int)(Permission.VerifyToken));

            var userPermsUpdated = await UserPermissionsDAO.GetByUserId(DbConnection, user.Id);
            Assert.Equal(userPermsUpdated.Count, 1);
            Assert.Equal(userPermsUpdated.First().UserId, user.Id);
            Assert.Equal(userPermsUpdated.First().Permission, Permission.VerifyToken);
        }

        [Fact]
        public async Task Delete_UserPermission_ShouldSucceed()
        {
            int permissionId = (int)(Permission.VerifyToken);
            var user = await UserDAOTests.CreateRandomUser(DbConnection);
            await UserPermissionsDAO.Create(DbConnection, user.Id, permissionId);

            List<UserPermission> userPermsOriginal = await UserPermissionsDAO.GetByUserId(DbConnection, user.Id);
            Assert.Equal(userPermsOriginal.Count, 1);
            Assert.Equal(userPermsOriginal.First().UserId, user.Id);
            Assert.Equal(userPermsOriginal.First().Permission, Permission.VerifyToken);
            
            await UserPermissionsDAO.Delete(DbConnection, user.Id, permissionId);
            List<UserPermission> userPermsUpdated = await UserPermissionsDAO.GetByUserId(DbConnection, user.Id);
            Assert.Equal(userPermsUpdated.Count, 0);
        }

        [Fact]
        public async Task Delete_ForInvalidUser_ShouldThrow()
        {
            int validPermissionId = (int)(Permission.VerifyToken);
            long invalidUserId = -1;

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await UserPermissionsDAO.Delete(DbConnection, invalidUserId, validPermissionId));
        }

        [Fact]
        public async Task Delete_ForInvalidPermission_ShouldThrow()
        {
            int invalidPermissionId = -1;
            long validUserId =  (await UserDAOTests.CreateRandomUser(DbConnection)).Id;

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await UserPermissionsDAO.Delete(DbConnection, validUserId, invalidPermissionId));
        }

        [Fact]
        public async Task GetByUserId_ShouldSucceed()
        {
            var user = await UserDAOTests.CreateRandomUser(DbConnection);
            List<UserPermission> userPermsOriginal = await UserPermissionsDAO.GetByUserId(DbConnection, user.Id);
            Assert.Equal(userPermsOriginal.Count, 0);
            
            await UserPermissionsDAO.Create(DbConnection, user.Id, (int)(Permission.VerifyToken));
            var userPermsUpdated = await UserPermissionsDAO.GetByUserId(DbConnection, user.Id);
            Assert.Equal(userPermsUpdated.First().UserId, user.Id);
        }

        [Fact]
        public async Task GetByUserId_ForUserWithoutPermissions_ShouldSucceed()
        {
            var user = await UserDAOTests.CreateRandomUser(DbConnection);
            List<UserPermission> userPermsOriginal = await UserPermissionsDAO.GetByUserId(DbConnection, user.Id);
            Assert.Equal(userPermsOriginal.Count, 0);
        }

        [Fact]
        public async Task GetUserEmailsWithPermission_ShouldSucceed()
        {
            Permission testPermission = Permission.ManageAnyUser;

            var userA = await UserDAOTests.CreateRandomUser(DbConnection);
            var userB = await UserDAOTests.CreateRandomUser(DbConnection);
            var userC = await UserDAOTests.CreateRandomUser(DbConnection);
            var userD = await UserDAOTests.CreateRandomUser(DbConnection);

            await UserPermissionsDAO.Create(DbConnection, userA.Id, (int)testPermission);
            await UserPermissionsDAO.Create(DbConnection, userB.Id, (int)testPermission);
            await UserPermissionsDAO.Create(DbConnection, userC.Id, (int)testPermission);
            await UserPermissionsDAO.Create(DbConnection, userD.Id, (int)Permission.ManageSelfUser);

            var firstPage = await UserPermissionsDAO.GetUserEmailsWithPermission(DbConnection, new List<Permission>{testPermission}, 0, 2);
            Assert.Equal(firstPage.Count, 2);
            Assert.Equal(firstPage[0].Item1, userA.Id);
            Assert.Equal(firstPage[0].Item2, userA.Email);
            Assert.Equal(firstPage[1].Item1, userB.Id);
            Assert.Equal(firstPage[1].Item2, userB.Email);
            
            var nextPage = await UserPermissionsDAO.GetUserEmailsWithPermission(
                DbConnection, new List<Permission>{testPermission}, firstPage[1].Item1, 2);

            Assert.Equal(nextPage.Count, 1);
            Assert.Equal(nextPage[0].Item1, userC.Id);
            Assert.Equal(nextPage[0].Item2, userC.Email);
        }
    }
}