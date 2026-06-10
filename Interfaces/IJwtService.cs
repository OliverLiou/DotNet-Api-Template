using System.Security.Claims;
using DotNetApiTemplate.Models.Entities;

namespace DotNetApiTemplate.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user, int? expiryHours = null, string tokenType = "access");
        ClaimsPrincipal GetPrincipalFromToken(string token, bool validateLifetime = true);
    }
}


