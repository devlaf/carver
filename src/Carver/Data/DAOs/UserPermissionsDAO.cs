using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Carver.Data.Models;
using Dapper;

namespace Carver.Data.DAOs
{
    public class UserPermissionsDAO
    {
        public static async Task<long> Create(IDbConnection dbConnection, long userId, int permissionId)
        {
            var sql = "INSERT INTO user_permissions (user_id, permission_id) VALUES (@UserId, @PermissionId) RETURNING id;";

            var values = new { UserId = userId , PermissionId = permissionId };
            
            return await Task.Factory.StartNew<long>(() => dbConnection.QueryFirst<long>(sql, values) );
        }

        public static async Task Delete(IDbConnection dbConnection, long userId, int permissionId)
        {
            var sql = "UPDATE user_permissions SET deleted_at=@DeletedAt WHERE user_id = @UserId AND permission_id = @PermissionId RETURNING id;";

            var values = new 
            {
                UserId = userId,
                PermissionId = permissionId,
                DeletedAt = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            };
            
            await Task.Factory.StartNew(() => dbConnection.QueryFirst(sql, values) );
        }

        public static async Task DeleteByUser(IDbConnection dbConnection, long userId)
        {
            var sql = "UPDATE user_permissions SET deleted_at=@DeletedAt WHERE user_id = @UserId;";

            var values = new 
            {
                UserId = userId,
                DeletedAt = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            };
            
            await Task.Factory.StartNew(() => dbConnection.Execute(sql, values) );
        }

        public static async Task<List<UserPermission>> GetByUserId(IDbConnection dbConnection, long userId)
        {
            var sql = "SELECT * FROM user_permissions WHERE user_id = @UserId AND deleted_at IS NULL;";

            var values = new { UserId = userId };
            
            return await Task.Factory.StartNew<List<UserPermission>>(() => {
                var result = dbConnection.Query(sql, values);
                return result.Select(x => new UserPermission(x.user_id, (Permission)x.permission_id)).AsList();
            });
        }

        public static async Task<List<Tuple<long, string>>> GetUserEmailsWithPermission(IDbConnection dbConnection, 
            List<Permission> targetPermissions, long pageFloor, int limit)
        {
            var sql = @"SELECT u.id, u.email FROM user_permissions up JOIN users u on u.id = up.user_id 
                        WHERE up.permission_id = ANY(@TargetPermissionIds) AND u.id > @PageFloor 
                        ORDER BY u.id LIMIT @Limit;";

            var values = new 
            { 
                TargetPermissionIds = targetPermissions.Cast<int>().AsList(),
                PageFloor = pageFloor,
                Limit = limit
            };
            
            return await Task.Factory.StartNew<List<Tuple<long, string>>>(() => {
                var result = dbConnection.Query(sql, values);
                return result.Select(x => new Tuple<long, string>(x.id, x.email)).AsList();
            });
        }
    }
}