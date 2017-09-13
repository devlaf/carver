using System;
using System.Data;
using System.Threading.Tasks;
using Carver.Data.Models;
using Dapper;

namespace Carver.Data.DAOs
{
    public static class UserDAO
    {
        public static async Task<long> Create(IDbConnection dbConnection, string userName, string email, 
            string hashedPassword, string salt, int hashingIterations, long createdAt)
        {
            var sql = @"INSERT INTO users (id, username, email, hashed_password, salt, hashing_iterations, created_at) " +
                "VALUES (DEFAULT, @Username, @Email, @HashedPassword, @Salt, @HashingIterations, @CreatedAt) " +
                "RETURNING id;";
        
            var values = new {
                Username = userName,
                Email = email,
                HashedPassword = hashedPassword,
                Salt = salt,
                HashingIterations = hashingIterations,
                CreatedAt = createdAt,
            };

            return await Task.Factory.StartNew<long>(() => {
                try { return dbConnection.QueryFirst<int>(sql, values); } 
                catch (Npgsql.PostgresException e) { throw new InvalidOperationException("Username already exists", e); }
            });
        }
        
        public static async Task<long> UpdateEmail(IDbConnection dbConnection, long userId, string email)
        {
            var sql = @"UPDATE users SET email=@Email WHERE id=@ID RETURNING id;";

            var values = new { ID = userId, Email = email };

            return await Task.Factory.StartNew<long>(() => dbConnection.QueryFirst<long>(sql, values));
        }

        public static async Task<string> GeneratePasswordResetToken(IDbConnection dbConnection, long userId)
        {
            var sql = @"UPDATE users SET password_reset_token=@PasswordResetToken WHERE id=@ID RETURNING password_reset_token;";

            var values = new { 
                ID = userId,
                PasswordResetToken = Guid.NewGuid()
            };

            return await Task.Factory.StartNew<string>(() => dbConnection.QueryFirst<string>(sql, values));

        }

        public static async Task<long> UpdatePassword(IDbConnection dbConnection, long userId, string passwordResetToken,
            string hashedPassword, string salt, int hashingIterations)
        {
            var sql = @"UPDATE users SET hashed_password=@HashedPassword, salt=@Salt, hashing_iterations=@HashingIterations 
                        WHERE id = @ID AND password_reset_token = @PasswordResetToken RETURNING id;";

            var values = new { 
                ID = userId,
                PasswordResetToken = passwordResetToken,
                HashedPassword = hashedPassword,
                Salt = salt,
                HashingIterations = hashingIterations
            };

            return await Task.Factory.StartNew<long>(() => dbConnection.QueryFirst<long>(sql, values));
        }

        public static async Task Deactivate(IDbConnection dbConnection, long userId)
        {
            var sql = @"UPDATE users SET deactivated_at=@DeactivatedAt WHERE id=@ID RETURNING id;";

            var values = new { 
                ID = userId,
                DeactivatedAt = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            };

            await Task.Factory.StartNew<long>(() => dbConnection.QueryFirst<long>(sql, values));
        }
        
        public static async Task<User?> GetById(IDbConnection dbConnection, long userId)
        {
            var sql = @"SELECT * FROM users WHERE id=@ID;";
            
            var values = new { ID = userId };

            return await Task.Factory.StartNew<User?>(() => {
                var result = dbConnection.QueryFirstOrDefault(sql, values);
                if (result == null)
                    return null;
                return new User(result.id, result.username, result.email, result.hashed_password, result.salt,
                    result.hashing_iterations, result.created_at, result.deactivated_at);
            });
        }

        public static async Task<User?> GetByUserName(IDbConnection dbConnection, string userName)
        {
            var sql = @"SELECT * FROM users WHERE username=@UserName;";
            
            var values = new { UserName = userName };

            return await Task.Factory.StartNew<User?>(() => {
                var result = dbConnection.QueryFirstOrDefault(sql, values);
                if (result == null)
                    return null;
                return new User(result.id, result.username, result.email, result.hashed_password, result.salt,
                    result.hashing_iterations, result.created_at, result.deactivated_at);
            });
        }
    }
}
