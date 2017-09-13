using System.Collections.Generic;
using System.Collections.Immutable;

namespace Carver.Data.Models
{
    public class UserPermissions
    {
        public long UserId { get; }

        public ImmutableList<Permission> Permissions { get; }

        public UserPermissions(long userId, List<Permission> permission)
        {
            UserId = userId;
            Permissions = ImmutableList.ToImmutableList(Permissions);
        }
    }
}