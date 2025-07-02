using Microsoft.AspNetCore.Identity;

namespace APIGateWay.Entities
{
    public class AppRole:IdentityRole<int>
    {
        public ICollection<AppUserRole> UserRole { get; set; }
    }
}
