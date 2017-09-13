namespace Carver.Data.Models
{
    public class Token
    {
        public string ID { get; set; }
        public string Description { get; set; }
        public long CreatedAt { get; set; }
        public long? ExpiresAt { get; set; }
        public long? RevokedAt { get; set; }
    }
}