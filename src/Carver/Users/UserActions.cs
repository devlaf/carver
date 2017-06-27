using System;
using System.Threading.Tasks;
using Carver.DataStore;
using log4net;

namespace Carver.Users
{
    internal static class UserActions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UserActions));

        internal static async Task<long> CreateNewUser(string username, string password, string email, UserGroup role)
        {
            ValidateEmail(email);
            var userIdentity = await UserAuthentication.GenerateUserCreds(username, password);

            Log.Info(string.Format("Creating new user [username={0} email={1}, role={2}", username, email,
                Enum.GetName(typeof(UserGroup), role)));

            IUserStore userStore = new PostgresDataStore();
            return await userStore.CreateUser(new User(username, email, userIdentity.HashedPassword, 
                userIdentity.Salt, userIdentity.HashingIterations, role, DateTime.Now, true));
        }

        public static async Task UpdateUserPassword(string username, string oldPassword, string newPassword)
        {
            if (!await ValidateUser(username, oldPassword))
                throw new ArgumentException("Specified username/oldPassword is not correct.");

            var newUserIdentity = await UserAuthentication.GenerateUserCreds(username, newPassword);
            Log.Info(string.Format("Updating password for user " + username));

            IUserStore userStore = new PostgresDataStore();
            var currentUser = (await userStore.GetUser(username)).Value;
            var currentUserId = (await userStore.GetUserId(username)).Value;

            await userStore.UpdateUser(currentUserId, new User(currentUser.Username, currentUser.Email, 
                newUserIdentity.HashedPassword, newUserIdentity.Salt, newUserIdentity.HashingIterations, 
                currentUser.UserGroup, currentUser.CreationDate, currentUser.Active));
        }

        public static async Task<bool> ValidateUser(string username, string password)
        {
            IUserStore userStore = new PostgresDataStore();

            var user = await userStore.GetUser(username);

            if (user == null)
                return false;

            if (!user.Value.Active)
                return false;

            var userCreds = new UserAuthentication.UserCredentials(user.Value.HashedPassword, 
                user.Value.Salt, user.Value.HashingIterations);

            return await (UserAuthentication.ConfirmUserPassword(userCreds, password));
        }

        private static void ValidateEmail(string emailAddress)
        {
            if (!emailAddress.Contains("@"))  // Clearly I'm being very thorough.
                throw new InvalidEmailException("The provided email did not specify a domain.");
        }

        internal class InvalidEmailException : Exception
        {
            public InvalidEmailException(string message) 
                : base(message) { }
            public InvalidEmailException(string message, Exception innerException) 
                : base(message, innerException) { }
        }
    }
}