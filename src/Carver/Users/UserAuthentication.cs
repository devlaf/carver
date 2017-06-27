using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Carver.Users
{
    /// <summary>
    /// A collection of static methods for managing passwords, roles, etc.
    /// </summary>
    /// <remarks>
    /// Strategy: PBKDF2 and 64 byte salt.  
    /// 
    /// PBKDF2 justification: 
    /// .NET framework's built-in PBKDF2 implementation has been cryptographically 
    /// verified, whereas none of the freely available .net implementations of bycrypt 
    /// have been.  Also, it is the NIST reccomendation.
    /// see http://nvlpubs.nist.gov/nistpubs/Legacy/SP/nistspecialpublication800-132.pdf
    /// </remarks>
    internal static class UserAuthentication
    {
        internal struct UserCredentials
        {
            public readonly string HashedPassword;
            public readonly string Salt;
            public readonly int HashingIterations;

            public UserCredentials(string hashedPassword, string salt, int hashingIterations)
            {
                HashedPassword = hashedPassword; Salt = salt; HashingIterations = hashingIterations;
            }
        }

        /// <summary>
        /// Generates a new UserCredentials object, including hashed password and salt, for the given user. This 
        /// method is expected to be used once for each user when they create an account.  The returned UserCredentials 
        /// object can be stored in the database and compared against for subsequent logins.
        /// </summary>
        /// <remarks>This method will take a long time to do password hashing (or it should). </remarks>
        public static async Task<UserCredentials> GenerateUserCreds(string username, string plaintextPassword)
        {
            const int SaltByteLength = 64;
            const int HashIterations = 100000;

            return await Task.Factory.StartNew<UserCredentials>(() =>
            {
                var pbkdf2 = new Rfc2898DeriveBytes(plaintextPassword, SaltByteLength, HashIterations);
                string hashedPassword = Convert.ToBase64String(pbkdf2.GetBytes(64));
                string salt = Convert.ToBase64String(pbkdf2.Salt);
                int iterations = pbkdf2.IterationCount;

                return new UserCredentials(hashedPassword, salt, iterations);
            });
        }

        /// <summary>
        /// Confirms that the hash of the provided plaintext password matches the information contained in the 
        /// credentials parameter.
        /// </summary>
        /// <param name="credentials">The hashed/salted information from the database against which to compare 
        /// the plaintext password.</param>
        /// <param name="password">The plaintext password.</param>
        /// <exception cref="FormatException">The HashedPassword or Salt values in the credentials
        /// parameter were not of the correct format -- Expected: Base64String </exception>
        public static async Task<bool> ConfirmUserPassword(UserCredentials credentials, string password)
        {
            return await Task.Factory.StartNew<bool>(() =>
            {
                var salt = Convert.FromBase64String(credentials.Salt);
                var passwordHash = Convert.FromBase64String(credentials.HashedPassword);
                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, credentials.HashingIterations);
                return pbkdf2.GetBytes(64).SequenceEqual(passwordHash);
            });
        }
    }
}