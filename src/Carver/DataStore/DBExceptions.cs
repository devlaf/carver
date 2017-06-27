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
}