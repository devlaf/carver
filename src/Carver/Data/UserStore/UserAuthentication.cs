using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Carver.Data.UserStore
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

        public static async Task<UserCredentials> GenerateUserCreds(string username, string plaintextPassword)
        {
            const int SaltByteLength = 64;
            const int HashingIterations = 100000;

            return await Task.Factory.StartNew<UserCredentials>(() =>
            {
                var pbkdf2 = new Rfc2898DeriveBytes(plaintextPassword, SaltByteLength, HashingIterations);
                string hashedPassword = Convert.ToBase64String(pbkdf2.GetBytes(64));
                string salt = Convert.ToBase64String(pbkdf2.Salt);
                int iterations = pbkdf2.IterationCount;

                return new UserCredentials(hashedPassword, salt, iterations);
            });
        }

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