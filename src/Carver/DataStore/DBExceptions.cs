using System;

namespace Carver
{
    /// <summary>
    /// An exception that represents errors connecting to a remote database.  Likely indicates an error with 
    /// the connection string.
    /// </summary>
    public class DatabaseConnectionException : Exception
    {
        public DatabaseConnectionException(string message)
            : base(message) { }
        public DatabaseConnectionException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class UserStoreException : Exception
    {
        public UserStoreException(string message)
            : base(message) { }
        public UserStoreException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class TokenStoreException : Exception
    {
        public TokenStoreException(string message)
            : base(message) { }
        public TokenStoreException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}