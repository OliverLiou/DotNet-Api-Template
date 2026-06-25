using System.DirectoryServices.Protocols;
using System.Net;
using DotNetApiTemplate.DTOs.Responses.User;
using DotNetApiTemplate.Interfaces;
using DotNetApiTemplate.Models.Context;
using DotNetApiTemplate.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DotNetApiTemplate.Services
{
    public class AuthService(TemplateContext context, IConfiguration configuration, UserManager<User> userManager) : IAuthService
    {
        private readonly TemplateContext _context = context;
        private readonly IConfiguration _configuration = configuration;
        private readonly UserManager<User> _userManager = userManager;

        /// <summary>
        /// 透過 LdapDirectoryIdentifier 與 LdapConnection 進行跨平台 AD 身分驗證與資料查詢。
        /// </summary>
        public Task<(bool IsValid, AdUserInfoDto? AdUserInfo, string? ErrorMessage)> AuthenticateAndFetchAdUserAsync(string username, string password)
        {
            try
            {
                var ldapServer = _configuration["LdapServer"]
                    ?? throw new InvalidOperationException("LdapServer configuration is required.");

                string domain = "";
                int firstDotIndex = ldapServer.IndexOf('.');
                if (firstDotIndex != -1)
                    domain = ldapServer.Substring(firstDotIndex + 1);

                var identifier = new LdapDirectoryIdentifier(ldapServer);
                using var connection = new LdapConnection(identifier);

                connection.SessionOptions.ProtocolVersion = 3;
                // 使用 Negotiate 可以在多數環境下自動與 AD 協調 Windows 整合驗證 (Kerberos/NTLM)
                connection.AuthType = AuthType.Negotiate;
                connection.Credential = new NetworkCredential(username, password, domain);
                connection.Bind();

                //查詢 Root DSE 取得 defaultNamingContext
                var rootDseRequest = new SearchRequest(
                    distinguishedName: string.Empty,
                    ldapFilter: "(objectClass=*)",
                    searchScope: SearchScope.Base,
                    attributeList: "defaultNamingContext"
                );

                var rootDseResponse = (SearchResponse)connection.SendRequest(rootDseRequest);
                if (rootDseResponse.Entries.Count == 0 || !rootDseResponse.Entries[0].Attributes.Contains("defaultNamingContext"))
                {
                    return Task.FromResult<(bool, AdUserInfoDto?, string?)>((false, null, "無法從 AD 取得 defaultNamingContext"));
                }

                var defaultNamingContext = rootDseResponse.Entries[0].Attributes["defaultNamingContext"][0].ToString();

                // 使用 sAMAccountName 進行搜尋以取得使用者資訊
                var searchFilter = $"(&(objectClass=user)(sAMAccountName={username}))";
                var searchRequest = new SearchRequest(
                    defaultNamingContext,
                    searchFilter,
                    SearchScope.Subtree,
                    "sn", "givenName", "mail", "displayName"
                );

                var searchResponse = (SearchResponse)connection.SendRequest(searchRequest);
                if (searchResponse.Entries.Count == 0)
                {
                    return Task.FromResult<(bool, AdUserInfoDto?, string?)>((false, null, $"找不到 AD 使用者: {username}"));
                }

                var entry = searchResponse.Entries[0];
                var adUserInfo = new AdUserInfoDto
                {
                    Surname = entry.Attributes.Contains("sn") ? entry.Attributes["sn"][0]?.ToString() : null,
                    GivenName = entry.Attributes.Contains("givenName") ? entry.Attributes["givenName"][0]?.ToString() : null,
                    EmailAddress = entry.Attributes.Contains("mail") ? entry.Attributes["mail"][0]?.ToString() : null,
                    DisplayName = entry.Attributes.Contains("displayName") ? entry.Attributes["displayName"][0]?.ToString() : null
                };

                return Task.FromResult<(bool, AdUserInfoDto?, string?)>((true, adUserInfo, null));
            }
            catch (LdapException ldapEx)
            {
                var message = ldapEx.ErrorCode switch
                {
                    49 => "帳號或密碼錯誤",
                    _ => $"AD 驗證失敗 (錯誤碼: {ldapEx.ErrorCode}): {ldapEx.Message}"
                };

                if (!string.IsNullOrEmpty(ldapEx.ServerErrorMessage))
                {
                    if (ldapEx.ServerErrorMessage.Contains("data 52e"))
                        message = "帳號或密碼錯誤";
                    else if (ldapEx.ServerErrorMessage.Contains("data 775"))
                        message = "帳號已被鎖定";
                    else if (ldapEx.ServerErrorMessage.Contains("data 533"))
                        message = "帳號已停用"; //這個會直接連線失敗
                    else if (ldapEx.ServerErrorMessage.Contains("data 532") || ldapEx.ServerErrorMessage.Contains("data 773"))
                        message = "密碼已過期或必須變更，請先變更密碼";
                    else if (ldapEx.ServerErrorMessage.Contains("data 701"))
                        message = "帳號已過期";
                    else
                        message = $"AD 驗證失敗: {ldapEx.ServerErrorMessage}";
                }

                return Task.FromResult<(bool, AdUserInfoDto?, string?)>((false, null, message));
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("The server is unavailable", StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult<(bool, AdUserInfoDto?, string?)>((false, null, "帳號或密碼錯誤"));
                }
                return Task.FromResult<(bool, AdUserInfoDto?, string?)>((false, null, $"AD 驗證發生錯誤: {ex.Message}"));
            }
        }

        public async Task<User?> GetUserByUserNameAsync(string userName) => await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);

        public async Task<User?> GetUserByIdAsync(string userId) => await _userManager.FindByIdAsync(userId);

        public async Task<IList<string>> GetUserRoleNamesAsync(User user) => await _userManager.GetRolesAsync(user);

        /// <summary>
        /// 驗證一般登入的帳號與密碼雜湊
        /// </summary>
        public async Task<(bool IsSuccess, User? User)> PasswordAuthenticateAsync(string username, string password)
        {
            var user = await GetUserByUserNameAsync(username);
            if (user == null)
            {
                return (false, null);
            }

            var isValid = await _userManager.CheckPasswordAsync(user, password);
            return (isValid, isValid ? user : null);
        }
    }
}
