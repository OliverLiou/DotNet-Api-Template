using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using  DotNetApiTemplate.Models;
using System.Threading.Tasks;

namespace JwtAuthDemo.Helpers
{
    public class JwtHelpers
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public JwtHelpers(IConfiguration configuration, UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<string> GenerateTokenAsync(string userName, int expireMinutes)
        {
            try
            {
                var issuer = _configuration.GetValue<string>("JwtSettings:Issuer");
                var signKey = _configuration.GetValue<string>("JwtSettings:SignKey");

                // 設定要加入到 JWT Token 中的聲明資訊(Claims)
                var claims = new List<Claim>();

                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userName)); // User.Identity.Name
                claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())); // JWT ID

                // 你可以自行擴充 "roles" 加入登入者該有的角色
                // claims.Add(new Claim("roles", "Admin"));
                // claims.Add(new Claim("roles", "Users"));
                var user = await _userManager.FindByNameAsync(userName);
                var roles = await _userManager.GetRolesAsync(user);
                foreach (var name in roles)
                {
                    claims.Add(new Claim("roles", name));
                }

                var userClaimsIdentity = new ClaimsIdentity(claims);

                // 建立一組對稱式加密的金鑰，主要用於 JWT 簽章之用
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signKey));

                // HmacSha256 有要求必須要大於 128 bits，所以 key 不能太短，至少要 16 字元以上
                // https://stackoverflow.com/questions/47279947/idx10603-the-algorithm-hs256-requires-the-securitykey-keysize-to-be-greater
                var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

                // 建立 SecurityTokenDescriptor
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Issuer = issuer,
                    //Audience = issuer, // 由於你的 API 受眾通常沒有區分特別對象，因此通常不太需要設定，也不太需要驗證
                    //NotBefore = DateTime.Now, // 預設值就是 DateTime.Now
                    //IssuedAt = DateTime.Now, // 預設值就是 DateTime.Now
                    Subject = userClaimsIdentity,
                    Expires = DateTime.Now.AddMinutes(expireMinutes),
                    SigningCredentials = signingCredentials
                };

                // 產出所需要的 JWT securityToken 物件，並取得序列化後的 Token 結果(字串格式)
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var serializeToken = tokenHandler.WriteToken(securityToken);

                return serializeToken;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string readTokenUser(string token)
        {
            return new JwtSecurityTokenHandler().ReadJwtToken(token).Subject;
        }
    }
}