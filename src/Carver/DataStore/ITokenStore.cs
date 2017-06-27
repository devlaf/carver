using System;
using System.Collections.Generic;
using System.Text;

namespace Carver.DataStore
{
    public interface ITokenStore
    {
        string CreateNewToken(string description, DateTime? expiration);

        void InvalidateToken(string token);

        bool TokenExists(string token);
    }
}
