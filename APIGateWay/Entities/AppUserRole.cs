using Microsoft.AspNetCore.Identity;

namespace APIGateWay.Entities
{
    public class AppUserRole:IdentityUserRole<int>
    {
        public App_User User { get; set; }
        public AppRole Role { get; set; }
    }
}
