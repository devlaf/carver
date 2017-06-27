using System;
using System.Collections.Generic;
using System.Text;

namespace Carver.DataStore.DataTypes
{
    internal struct User
    {
        public string Username { get; }
        public string Email { get; }
        public string HashedPassword { get; }
        public string Salt { get; }
        public long HashingIterations { get; }
        public UserGroup UserGroup { get; }
        public DateTime CreationDate { get; }

        public User(string username, string email, string hashedPassword, string salt, long hashingIterations, 
            UserGroup userGroup, DateTime creationDate)
        {
            Username = username;
            Email = email;
            HashedPassword = hashedPassword;
            Salt = salt;
            HashingIterations = hashingIterations;
            UserGroup = userGroup;
            CreationDate = creationDate;
        }
    }
}
