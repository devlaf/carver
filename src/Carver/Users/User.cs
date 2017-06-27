using System;
using System.Collections.Generic;
using System.Text;

namespace Carver.Users
{
    internal struct User
    {
        public string Username { get; }
        public string Email { get; }
        public string HashedPassword { get; }
        public string Salt { get; }
        public int HashingIterations { get; }
        public UserGroup UserGroup { get; }
        public DateTime CreationDate { get; }
        public bool Active { get; }

        public User(string username, string email, string hashedPassword, string salt, int hashingIterations, 
            UserGroup userGroup, DateTime creationDate, bool active)
        {
            Username = username;
            Email = email;
            HashedPassword = hashedPassword;
            Salt = salt;
            HashingIterations = hashingIterations;
            UserGroup = userGroup;
            CreationDate = creationDate;
            Active = active;
        }
    }
}