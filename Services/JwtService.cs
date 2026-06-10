using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DotNetApiTemplate.Models.Entities;
using DotNetApiTemplate.Interfaces;
using DotNetApiTemplate.Settings;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;

namespace DotNetApiTemplate.Services
{
    public class JwtService(IOptions<JwtSettings> jwtOptions) : IJwtService
    {
        private readonly JwtSettings _jwtSettings = jwtOptions.Value;

        /// <summary>
        /// 根據使用者資訊生成 JWT token
        /// </summary>
        public string GenerateToken(User user, int? expiryHours = null, string tokenType = JwtTokenTypes.Access)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>()
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.EmployeeName ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new("token_type", tokenType),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expiryHours ?? _jwtSettings.ExpiryInHours),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal GetPrincipalFromToken(string token, bool validateLifetime = true)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = false,
                ValidateLifetime = validateLifetime,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey))
            }, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                !string.Equals(jwtToken.Header.Alg, SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
            {
                throw new SecurityTokenException("Token 格式無效");
            }

            return principal;
        }
    }
}

