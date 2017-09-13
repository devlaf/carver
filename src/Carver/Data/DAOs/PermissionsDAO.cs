using System;
using System.Data;
using System.Threading.Tasks;
using Carver.Data.Models;
using Dapper;

namespace Carver.Data.DAOs
{
    public class PermissionsDAO
    {
        public static async Task<Permission> Get(IDbConnection dbConnection, long id)
        {
            var sql = @"select * from permissions where id = @Id;";
        
            var values = new { Id = id };

            return await Task.Factory.StartNew<Permission>(() => {
                dynamic result = dbConnection.QueryFirst(sql, values);
                return (Permission)result.id;
            });
        }
    }
}