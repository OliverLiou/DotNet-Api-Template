using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DotNetApiTemplate.DTOs.Entities;
using DotNetApiTemplate.DTOs.Settings;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;

namespace DotNetApiTemplate.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }

    public class JwtService(IOptions<JwtSettings> jwtOptions) : IJwtService
    {
        private readonly JwtSettings _jwtSettings = jwtOptions.Value;

        /// <summary>
        /// 根據使用者資訊生成 JWT token
        /// </summary>
        public string GenerateToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>()
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.EmployeeName ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpiryInHours),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
