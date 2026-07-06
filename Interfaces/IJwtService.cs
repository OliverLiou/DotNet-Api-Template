using System.Security.Claims;
using DotNetWebApiMssql.Models.Entities;

namespace DotNetWebApiMssql.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user, int? expiryHours = null, string tokenType = "access");
        ClaimsPrincipal GetPrincipalFromToken(string token, bool validateLifetime = true);
    }
}


