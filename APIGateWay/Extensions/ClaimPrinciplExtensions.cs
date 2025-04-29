using System.Security.Claims;

namespace APIGateWay.Extensions
{
    public static class ClaimPrinciplExtensions
    {
        public static string GetUsername(this ClaimsPrincipal user)
        {
           return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
