using System.Text;
using System.Security.Claims;
using DotNetApiTemplate.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using DotNetApiTemplate.ViewModels;

namespace DotNetApiTemplate.Services
{
    public interface IAuthService
    {
        Task<User?> CheckUserExistsAsync(string googleId);
        Task<User> CreateUserAsync(VGoogleUserInfo vGoogleUserInfo);
        Task<User> UpdateUserAsync(User user);
    }

    public class AuthService(TemplateContext context) : IAuthService
    {
        private readonly TemplateContext _context = context;

        public async Task<User?> CheckUserExistsAsync(string googleId)
        {
            try
            {
                // 檢查使用者是否已存在
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
                return existingUser;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<User> CreateUserAsync(VGoogleUserInfo vGoogleUserInfo)
        {
            try
            {
                var user = new User()
                {
                    GoogleId = vGoogleUserInfo.GoogleId,
                    Email = vGoogleUserInfo.Email,
                    UserName = vGoogleUserInfo.Name,
                    Picture = vGoogleUserInfo.Picture,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    IsActive = true,
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return user;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<User> UpdateUserAsync(User user_modified)
        { 
            try
            {
                _context.Users.Update(user_modified);
                await _context.SaveChangesAsync();
                return user_modified;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}