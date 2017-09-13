namespace Carver.Data.Models
{
    public enum Permission
    {
        ManageAnyUser,
        ManageSelfUser,
        CreateToken,
        InvalidateToken,
        VerifyToken
    }
}