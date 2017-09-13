namespace Carver.Data.Models
{
    public class UserPermission
    {
        public long UserId { get; set; }

        public Permission Permission { get; set; }
        
        public UserPermission(long userId, Permission permission)
        {
            UserId = userId;
            Permission = permission;
        }
    }
}