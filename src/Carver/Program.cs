using System;
using Carver.DataStore;
using Carver.Users;

namespace Carver
{
    class Program
    {
        static void Main(string[] args)
        {
            //IUserStore store = new PostgresDataStore();

            //var barry = new User("barry2", "barry@aol.net", "abcdef", "abcd", 4, UserGroup.admin, DateTime.Now, true);

            //var id = store.CreateUser(barry);
            ////var id = store.GetUserId("barry").Value;

            //var newBarry = new User("barry2", "barry@gmail.com", "abcdef", "abcd", 4, UserGroup.admin, DateTime.Now, true);

            //store.UpdateUser(id, newBarry);

            //ITokenStore store = new PostgresDataStore();
            //var token = store.CreateNewToken("blah", null);
            //Console.WriteLine(token);

            //store.InvalidateToken(token);

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}