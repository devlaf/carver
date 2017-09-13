using System;
using System.Data;
using System.Threading.Tasks;
using Carver.Data.Models;
using Carver.Data.DAOs;
using Xunit;

namespace CarverTests.DAOTests
{
    public class PermissionsDAOTests
    {
        private IDbConnection DbConnection;

        public PermissionsDAOTests()
        {
            DbConnection = new IntegrationTestsDbConnection().DbConnection;
        }

        [Fact]
        public async Task Get_Should_Succeed()
        {
            foreach (var permissionId in Enum.GetValues(typeof(Permission)))
            {
                var permission = await PermissionsDAO.Get(DbConnection, (int)permissionId);
                Assert.Equal(permission, (Permission)permissionId);
            }
        }
    }
}