using System;
using System.Collections.Generic;
using System.Text;
using Carver.Users;
using Xunit;

namespace CarverTests
{
    public class UserAuthenticationTests
    {
        [Fact]
        public async void GenerateUserCreds_Should_Succeed()
        {
            var userCreds = await UserAuthentication.GenerateUserCreds("snoopy", "myPassword");
            Assert.NotEqual(userCreds.HashedPassword, "myPassword");
        }

        [Fact]
        public async void MultipleUserCreds_Should_BeDistinct()
        {
            var userCreds1 = await UserAuthentication.GenerateUserCreds("linus", "blanket");
            var userCreds2 = await UserAuthentication.GenerateUserCreds("linus", "blanket");
            Assert.NotEqual(userCreds1.HashedPassword, userCreds2.HashedPassword);
            Assert.NotEqual(userCreds1.Salt, userCreds2.Salt);
            Assert.Equal(userCreds1.HashingIterations, userCreds2.HashingIterations);
        }

        [Fact]
        public async void ConfirmUserPassword_Should_Succeed()
        {
            var userCreds = await UserAuthentication.GenerateUserCreds("woodstock", "doghouse");
            var validUser = await UserAuthentication.ConfirmUserPassword(userCreds, "doghouse");

            Assert.True(validUser);
        }
    }
}
