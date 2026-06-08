using System.Text;
using System.Security.Claims;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Runtime.InteropServices;
using DotNetApiTemplate.DTOs.Context;
using DotNetApiTemplate.DTOs.Entities;
using DotNetApiTemplate.DTOs.Interfaces;
using DotNetApiTemplate.DTOs.ViewModels.User;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;


namespace DotNetApiTemplate.Services
{
    public class AuthService(TemplateContext context, IConfiguration configuration, UserManager<User> userManager) : IAuthService
    {
        private readonly TemplateContext _context = context;
        private readonly IConfiguration _configuration = configuration;
        private readonly UserManager<User> _userManager = userManager;

        /// <summary>
        /// 透過 DirectoryEntry (LDAP Bind) 驗證使用者帳號密碼，
        /// 僅做身分驗證，不抓取使用者資料。
        /// </summary>
        public Task<(bool IsValid, string? ErrorMessage)> AdAuthenticateAsync(string username, string password)
        {
            try
            {
                var domainAd = _configuration["DomainAD"]
                    ?? throw new InvalidOperationException("DomainAD configuration is required.");

                var ldapPath = $"LDAP://{domainAd}";

                // 需要在建構子中明確指定  AuthenticationTypes ，強制它不使用當前  Windows 身分的 Kerberos Ticket
                using var entry = new DirectoryEntry(ldapPath, username, password, AuthenticationTypes.Secure);
                // 強制觸發 LDAP Bind，驗證帳號密碼是否正確
                _ = entry.NativeObject;

                return Task.FromResult<(bool, string?)>((true, null));
            }
            catch (COMException comEx)
            {
                var errorCode = unchecked((uint)comEx.ErrorCode);
                var message = errorCode switch
                {
                    0x8007052E => "帳號或密碼錯誤",                         // ERROR_LOGON_FAILURE
                    0x80070775 => "帳號已被鎖定，請聯繫管理員",               // ERROR_ACCOUNT_LOCKED_OUT
                    0x80070533 => "帳號已停用",                             // ERROR_ACCOUNT_DISABLED
                    0x80070532 => "密碼已過期，請先變更密碼",                 // ERROR_PASSWORD_EXPIRED
                    0x80070773 => "密碼必須變更，請先至系統變更密碼",          // ERROR_PASSWORD_MUST_CHANGE
                    0x8007052F => "帳號目前有登入限制，請聯繫管理員",          // ERROR_ACCOUNT_RESTRICTION
                    0x80070774 => "帳號已過期",                             // ERROR_ACCOUNT_EXPIRED
                    0x80070701 => "帳號的登入時間已受限制",                   // ERROR_INVALID_LOGON_HOURS
                    0x80070569 => "登入失敗：帳號類型限制",                   // ERROR_LOGON_TYPE_NOT_GRANTED
                    _          => $"AD 驗證失敗 (錯誤碼: 0x{errorCode:X8}): {comEx.Message}",
                };
                return Task.FromResult<(bool, string?)>((false, message));
            }
            catch (Exception ex)
            {
                return Task.FromResult<(bool, string?)>((false, $"AD 驗證發生錯誤: {ex.Message}"));
            }
        }

        /// <summary>
        /// 透過 PrincipalContext 及 FindByIdentity 查詢 AD 使用者資料，
        /// 回傳 AdUserInfoDto（已在 context 存活期間擷取資料）。
        /// </summary>
        public Task<(AdUserInfoDto? AdUserInfo, string? ErrorMessage)> FetchAdUserPrincipal(string username)
        {
            try
            {
                var domainAd = _configuration["DomainAD"]
                    ?? throw new InvalidOperationException("DomainAD configuration is required.");

                using var principalContext = new PrincipalContext(ContextType.Domain, domainAd);
                using var userPrincipal = UserPrincipal.FindByIdentity(principalContext, IdentityType.SamAccountName, username);

                if (userPrincipal == null)
                    return Task.FromResult<(AdUserInfoDto?, string?)>((null, $"找不到使用者 {username}"));

                // 在 PrincipalContext 存活期間擷取所需資料至 DTO
                var adUserInfo = new AdUserInfoDto
                {
                    Surname = userPrincipal.Surname,
                    GivenName = userPrincipal.GivenName,
                    EmailAddress = userPrincipal.EmailAddress,
                    DisplayName = userPrincipal.DisplayName
                };

                return Task.FromResult<(AdUserInfoDto?, string?)>((adUserInfo, null));
            }
            catch (PrincipalServerDownException)
            {
                return Task.FromResult<(AdUserInfoDto?, string?)>((null, "AD 伺服器無法連線，請稍後再試"));
            }
            catch (Exception ex)
            {
                return Task.FromResult<(AdUserInfoDto?, string?)>((null, $"查詢 AD 使用者資料發生錯誤: {ex.Message}"));
            }
        }

        public async Task<User?> GetUserByUserNameAsync(string userName)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            try
            {
                return await _userManager.FindByIdAsync(userId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IList<string>> GetUserRoleNamesAsync(User user)
        {
            try
            {
                return await _userManager.GetRolesAsync(user);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}