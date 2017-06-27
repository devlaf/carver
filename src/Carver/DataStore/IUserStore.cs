using System;
using System.Collections.Generic;
using System.Text;

namespace Carver.DataStore
{
    internal interface IUserStore
    {
        long CreateUser(DataTypes.User user);

        long UpdateUser(long userId, DataTypes.User user);

        void InvalidateUser(long userId);

        long GetUserId(string userName);
    }
}
