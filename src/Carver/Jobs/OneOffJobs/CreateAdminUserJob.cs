using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using Carver.Data;
using Carver.Data.Models;
using Carver.Data.UserStore;
using log4net;

namespace Carver.Jobs.OneOffJobs
{
    public class CreateAdminUserJob : IOneOffJob
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CreateAdminUserJob));
        private IUserStore UserStore = DataStoreFactory.UserDataStore;

        private bool dryRun;
        private string userName;
        private string email;
        private string password;

        public string Usage()
        {
            var result = new StringBuilder();
            result.AppendLine("--dryRun        required=false");
            result.AppendLine("--userName=XXX  required=true");
            result.AppendLine("--password=XXX  required=true");
            result.AppendLine("--email=XXX     required=true");
            return result.ToString();
        }

        public void SetOptions(string args)
        {
            var parts = args.Split(null);

            foreach (var part in parts)
            {
                AssessArg(part);
            }
        }

        private void AssessArg(string arg)
        {
            if (arg.Contains("--dryRun"))
                dryRun = true;
            else if (arg.Contains("--userName="))
                userName = arg.Replace("--userName=", "");
            else if (arg.Contains("--password="))
                password = arg.Replace("--password=", "");
            else if (arg.Contains("--email="))
                email = arg.Replace("--email=", "");
            else
                throw new ArgumentException("Usage: " + Environment.NewLine + Usage());
        }

        public void Run()
        {
            var user = UserStore.CreateUser(userName, password, email).Result;
            UserStore.EnsurePermissionsForUser(user, new HashSet<Permission>{ Permission.ManageAnyUser }).Wait();
            Log.Info($"Created user {user} with the ManageAnyUser permission");
        }
    }
}