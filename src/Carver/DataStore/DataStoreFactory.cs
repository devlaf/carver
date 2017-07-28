using System;
using System.Collections.Generic;
using System.Text;

namespace Carver.DataStore
{
    internal static class DataStoreFactory
    {
        internal static ITokenStore TokenDataStore => new PostgresDataStore();
        internal static IUserStore UserDataStore => new PostgresDataStore();
    }
}
