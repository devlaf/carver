using System;
using System.Threading.Tasks;
using Carver.Data.UserStore;
using Carver.Data.Models;
using Xunit;

namespace CarverTests
{
    public class UserStoreTests
    {
        private readonly IUserStore UserStore;

        public UserStoreTests()
        {
            IntegrationTestsDbConnection TestConnection = new IntegrationTestsDbConnection();
            UserStore = new UserStore(TestConnection.DbConnection);
        }

        [Fact]
        public async Task CreateNewUser_Should_Succeed()
        {
            string username = "Foghorn Leghorn";
            string password = "W@lkyT@lkyH@wky";
            string email = "Foggy@acme.com";

            var user = await UserStore.GetUser(username);
            Assert.Null(user);
            var userId = await UserStore.CreateUser(username, password, email);
            user = await UserStore.GetUser(username);
            Assert.NotNull(user);
            Assert.Equal(user.Value.Username, username);
            Assert.Equal(user.Value.Email, email);
            Assert.NotEqual(user.Value.HashedPassword, password);
        }

        [Fact]
        public async Task CreateNewUser_Should_SanityCheckEmail()
        {
            string username = "Marvin The Martian";
            string password = "3@rthsux";
            string email = "marvin_at_ma.rs";
            
            await Assert.ThrowsAsync<UserStore.InvalidEmailException>(async () => await UserStore.CreateUser(username, password, email));
        }

        [Fact]
        public async void CreateNewUser_Should_StoreValidUserCreds()
        {
            string username = "Speedy";
            string password = "zoom";
            string email = "faster_than_email@acme.com";

            long userId = await UserStore.CreateUser(username, password, email);
            var user = await UserStore.GetUser(userId);

            Assert.NotEqual(user?.HashedPassword, password);
        }

        [Fact]
        public async Task CreateNewUser_ShouldThrow_ForExistingUsername()
        {
            string username = "Porky";
            string password = "ismellbacon";
            string email = "thatsall@folks.com";

            long userId = await UserStore.CreateUser(username, password, email);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await UserStore.CreateUser(username, password, email));
        }

        [Fact]
        public async Task UpdateUserPassword_Should_Succeed()
        {
            string username = "bugs_bunny";
            string firstPassword = "fudd_it_up";
            string secondPassword = "carrots";
            string email = "bugs@acme.co";

            long userId = await UserStore.CreateUser(username, firstPassword, email);
            Assert.True(await UserStore.ValidateUser(username, firstPassword));

            await UserStore.UpdateUserPassword(username, firstPassword, secondPassword);
            Assert.False(await UserStore.ValidateUser(username, firstPassword));
            Assert.True(await UserStore.ValidateUser(username, secondPassword));
        }

        [Fact]
        public async void UpdateUserPassword_Should_FailForInvalidOldPassword()
        {
            string username = "elmer";
            string password = "killthewabbit";
            string email = "elmer@acme.co";

            long userId = await UserStore.CreateUser(username, password, email);
            Assert.True(await UserStore.ValidateUser(username, password));

            await Assert.ThrowsAsync<ArgumentException>(async () => await UserStore.UpdateUserPassword(username, "blah", "blee"));
        }

        [Fact]
        public async void UpdateUser_Should_FailForNonExistantUser()
        {
            await Assert.ThrowsAsync<ArgumentException>(async () => await UserStore.UpdateUserPassword("invalid_username", "", ""));
        }

        [Fact]
        public async void UpdateUser_Should_FailForNullUser()
        {
            await Assert.ThrowsAsync<ArgumentException>(async () => await UserStore.UpdateUserPassword(null, "", ""));
        }

        [Fact]
        public async void ValidateUser_ShouldReturnFalse_NonExistantUser()
        {
            Assert.False( await UserStore.ValidateUser("invalid_username", "") );
        }

        [Fact]
        public async void ValidateUser_ShouldReturnFalse_ForNullUser()
        {
            Assert.False( await UserStore.ValidateUser(null, "") );
        }

        [Fact]
        public async void ValidateUser_ShouldReturnFalse_ForDeactivatedUser()
        {
            string username = "road_runner";
            string password = "boom";
            string email = "rr@acme.co";

            long userId = await UserStore.CreateUser(username, password, email);
            Assert.True(await UserStore.ValidateUser(username, password));

            await UserStore.DeactivateUser(userId);
            Assert.False(await UserStore.ValidateUser(username, password));
        }
    }
}