namespace Carver.Data.Models
{
    public struct User
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string HashedPassword { get; set; }
        public string Salt { get; set; }
        public int HashingIterations { get; set; }
        public long CreatedAt { get; set; }
        public long? DeactivatedAt { get; set; }

        public User(long id, string username, string email, string hashedPassword, string salt,
            int hashingIterations, long createdAt, long? deactivatedAt)
        {
            Id = id;
            Username = username;
            Email = email;
            HashedPassword = hashedPassword;
            Salt = salt;
            HashingIterations = hashingIterations;
            CreatedAt = createdAt;
            DeactivatedAt = deactivatedAt;
        }
    }
}