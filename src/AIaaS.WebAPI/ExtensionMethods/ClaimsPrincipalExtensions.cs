using System.Security.Claims;

namespace AIaaS.WebAPI.ExtensionMethods
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetEmail(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal?.FindFirstValue(ClaimTypes.Email);
        }
    }
}
