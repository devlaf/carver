using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Carver.DataStore;
using Carver.Users;
using Moq;
using Xunit;

namespace CarverTests
{
    public class UserActionTests
    {
        private IUserStore GetMockedUserStore(Func<User, long> createNewUserFunc = null, 
                                              Func<long, User, long> updateUserFunc = null,
                                              Func<string, User?> getUserFunc = null,
                                              Func<string, long?> getUserIdFunc = null)
        {
            var mock = new Mock<IUserStore>();

            mock.Setup(x => x.CreateUser(It.IsAny<User>()))
                .Returns<User>(x => Task.FromResult(createNewUserFunc(x)));

            mock.Setup(x => x.UpdateUser(It.IsAny<long>(), It.IsAny<User>()))
                .Returns<long, User>((x, y) => Task.FromResult(updateUserFunc(x, y)));

            mock.Setup(x => x.GetUser(It.IsAny<string>()))
                .Returns<string>(x => Task.FromResult(getUserFunc(x)));

            mock.Setup(x => x.GetUserId(It.IsAny<string>()))
                .Returns<string>(x => Task.FromResult(getUserIdFunc(x)));

            return mock.Object;
        }

        [Fact]
        public async void CreateNewUser_Should_Succeed()
        {
            IUserStore userStore = GetMockedUserStore(createNewUserFunc: x => 6);

            var userId = await UserActions.CreateNewUser(userStore, "Foghorn Leghorn", "W@lkyT@lkyH@wky", "Foggy@acme.com", UserGroup.user);

            Assert.Equal(userId, 6);
        }

        [Fact]
        public void CreateNewUser_Should_SanityCheckEmail()
        {
            IUserStore userStore = GetMockedUserStore(createNewUserFunc: x => 6);

            Assert.ThrowsAsync<UserActions.InvalidEmailException>(async () => await UserActions.CreateNewUser(userStore, 
                "Marvin The Martian", "3@rthsux", "marvin_at_ma.rs", UserGroup.user));
        }

        [Fact]
        public async void CreateNewUser_Should_StoreValidUserCreds()
        {
            IUserStore userStore = GetMockedUserStore(createNewUserFunc: x => (x.HashedPassword == "zoom" ? 0 : 1));
            
            var userId = await UserActions.CreateNewUser(userStore, "Speedy", "zoom", "faster_than_email@acme.com", UserGroup.user);

            Assert.Equal(userId, 1);
        }

        [Fact]
        public async void UpdateUserPassword_Should_Succeed()
        {
            var initialUserCreds = await UserAuthentication.GenerateUserCreds("bugs_bunny", "fudd_it_up");
            var user = new User("bugs_bunny", "bugs@acme.co", initialUserCreds.HashedPassword, initialUserCreds.Salt, 
                initialUserCreds.HashingIterations, UserGroup.user, DateTime.Now, true);

            IUserStore userStore = GetMockedUserStore(
                updateUserFunc: (id, username) => id, 
                getUserFunc: x => user,
                getUserIdFunc: x => 1);

            await UserActions.UpdateUserPassword(userStore, "bugs_bunny", "fudd_it_up", "carrots");
        }

        [Fact]
        public async void UpdateUserPassword_Should_FailForInvalidOldPassword()
        {
            var initialUserCreds = await UserAuthentication.GenerateUserCreds("bugs_bunny", "fudd_it_up");
            var user = new User("bugs_bunny", "bugs@acme.co", initialUserCreds.HashedPassword, initialUserCreds.Salt,
                initialUserCreds.HashingIterations, UserGroup.user, DateTime.Now, true);

            IUserStore userStore = GetMockedUserStore(
                updateUserFunc: (id, username) => id,
                getUserFunc: x => user,
                getUserIdFunc: x => 1);

            Assert.ThrowsAsync<ArgumentException>(async () => await UserActions.UpdateUserPassword(userStore, 
                "bugs_bunny", "invalid_pw", "carrots"));
        }

        [Fact]
        public async void Validateuser_Should_FailForNullUser()
        {
            IUserStore userStore = GetMockedUserStore();
            Assert.ThrowsAsync<ArgumentException>(async () => await UserActions.UpdateUserPassword(userStore, null, "", ""));
        }

        [Fact]
        public async void Validateuser_Should_FailForInvalidatedUser()
        {
            var initialUserCreds = await UserAuthentication.GenerateUserCreds("bugs_bunny", "fudd_it_up");
            var user = new User("bugs_bunny", "bugs@acme.co", initialUserCreds.HashedPassword, initialUserCreds.Salt,
                initialUserCreds.HashingIterations, UserGroup.user, DateTime.Now, false);

            IUserStore userStore = GetMockedUserStore(
                updateUserFunc: (id, username) => id,
                getUserFunc: x => user,
                getUserIdFunc: x => 1);

            Assert.ThrowsAsync<ArgumentException>(async () => await UserActions.UpdateUserPassword(userStore,
                "bugs_bunny", "invalid_pw", "carrots"));
        }
    }
}
