using System.DirectoryServices.Protocols;
using System.Net;
using DotNetWebApiMssql.DTOs.Responses.User;
using DotNetWebApiMssql.Interfaces;
using DotNetWebApiMssql.Models.Context;
using DotNetWebApiMssql.Models.Entities;
using DotNetWebApiMssql.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace DotNetWebApiMssql.Services
{
    public class AuthService(
        TemplateContext context,
        IOptions<LdapSettings> ldapOptions,
        UserManager<User> userManager,
        ILogger<AuthService> logger) : IAuthService
    {
        private readonly TemplateContext _context = context;
        private readonly LdapSettings _ldapSettings = ldapOptions.Value;
        private readonly UserManager<User> _userManager = userManager;
        private readonly ILogger<AuthService> _logger = logger;

        /// <summary>
        /// 透過 ServiceUsername 與 ServicePassword 查詢 AD 帳號是否存在 (不做密碼驗證)
        /// </summary>
        public Task<(bool IsValid, string? ErrorMessage)> CheckAdAccountExistAsync(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return Task.FromResult((false, (string?)"帳號或密碼錯誤"));
                }

                var ldapServer = _ldapSettings.Server;
                if (string.IsNullOrWhiteSpace(ldapServer))
                {
                    throw new InvalidOperationException("LdapSettings:Server configuration is required.");
                }

                var serviceUsername = _ldapSettings.ServiceUsername;
                var servicePassword = _ldapSettings.ServicePassword;
                if (string.IsNullOrWhiteSpace(serviceUsername) || string.IsNullOrWhiteSpace(servicePassword))
                {
                    throw new InvalidOperationException("LdapSettings Service credentials are required.");
                }

                string domain = "";
                int firstDotIndex = ldapServer.IndexOf('.');
                if (firstDotIndex != -1)
                    domain = ldapServer.Substring(firstDotIndex + 1);

                var identifier = new LdapDirectoryIdentifier(ldapServer);
                using var connection = new LdapConnection(identifier);

                connection.SessionOptions.ProtocolVersion = 3;
                connection.AuthType = AuthType.Negotiate;
                connection.Credential = new NetworkCredential(serviceUsername, servicePassword, domain);
                connection.Bind();

                // 查詢 Root DSE 取得 defaultNamingContext
                var rootDseRequest = new SearchRequest(
                    distinguishedName: string.Empty,
                    ldapFilter: "(objectClass=*)",
                    searchScope: SearchScope.Base,
                    attributeList: LdapAttributeSettings.defaultNamingContext
                );

                var rootDseResponse = (SearchResponse)connection.SendRequest(rootDseRequest);
                if (rootDseResponse.Entries.Count == 0 || !rootDseResponse.Entries[0].Attributes.Contains(LdapAttributeSettings.defaultNamingContext))
                {
                    _logger.LogError("無法從 AD 取得 defaultNamingContext。");
                    return Task.FromResult((false, (string?)"帳號或密碼錯誤"));
                }

                var defaultNamingContext = rootDseResponse.Entries[0].Attributes[LdapAttributeSettings.defaultNamingContext][0].ToString();

                // 使用 sAMAccountName 進行搜尋以確認使用者是否存在
                var searchFilter = $"(&(objectClass=user)({LdapAttributeSettings.sAMAccountName}={username}))";
                var searchRequest = new SearchRequest(
                    defaultNamingContext,
                    searchFilter,
                    SearchScope.Subtree,
                    LdapAttributeSettings.sAMAccountName
                );

                var searchResponse = (SearchResponse)connection.SendRequest(searchRequest);
                if (searchResponse.Entries.Count == 0)
                {
                    _logger.LogWarning("找不到 AD 使用者: {Username}", username);
                    return Task.FromResult((false, (string?)"帳號或密碼錯誤"));
                }

                return Task.FromResult((true, (string?)null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CheckAdAccountExistAsync 發生異常，帳號: {Username}", username);
                return Task.FromResult((false, (string?)"帳號或密碼錯誤"));
            }
        }

        /// <summary>
        /// 透過 LdapDirectoryIdentifier 與 LdapConnection 進行跨平台 AD 身分驗證與資料查詢。
        /// </summary>
        public async Task<(bool IsValid, AdUserInfoDto? AdUserInfo, string? ErrorMessage)> AuthenticateAndFetchAdUserAsync(string username, string password)
        {
            // 1. 先呼叫 CheckAdAccountExistAsync 檢查帳號是否存在
            var (isExist, existErrorMessage) = await CheckAdAccountExistAsync(username);
            if (!isExist)
            {
                // 如果帳號不存在，直接回傳模糊錯誤訊息
                // (注意：錯誤原因已在 CheckAdAccountExistAsync 中被記錄在 Log)
                return (false, null, existErrorMessage ?? "帳號或密碼錯誤");
            }

            try
            {
                var ldapServer = _ldapSettings.Server;
                if (string.IsNullOrWhiteSpace(ldapServer))
                {
                    throw new InvalidOperationException("LdapSettings:Server configuration is required.");
                }

                string domain = "";
                int firstDotIndex = ldapServer.IndexOf('.');
                if (firstDotIndex != -1)
                    domain = ldapServer.Substring(firstDotIndex + 1);

                var identifier = new LdapDirectoryIdentifier(ldapServer);
                using var connection = new LdapConnection(identifier);

                connection.SessionOptions.ProtocolVersion = 3;
                connection.AuthType = AuthType.Negotiate;
                connection.Credential = new NetworkCredential(username, password, domain);
                // 這裡會用傳入的密碼進行驗證
                connection.Bind();

                // 2. 驗證成功後，取得 defaultNamingContext
                var rootDseRequest = new SearchRequest(
                    distinguishedName: string.Empty,
                    ldapFilter: "(objectClass=*)",
                    searchScope: SearchScope.Base,
                    attributeList: LdapAttributeSettings.defaultNamingContext
                );

                var rootDseResponse = (SearchResponse)connection.SendRequest(rootDseRequest);
                if (rootDseResponse.Entries.Count == 0 || !rootDseResponse.Entries[0].Attributes.Contains(LdapAttributeSettings.defaultNamingContext))
                {
                    _logger.LogError("無法從 AD 取得 defaultNamingContext");
                    return (false, null, "帳號或密碼錯誤");
                }

                var defaultNamingContext = rootDseResponse.Entries[0].Attributes[LdapAttributeSettings.defaultNamingContext][0].ToString();

                // 3. 查詢使用者詳細資料
                var searchFilter = $"(&(objectClass=user)({LdapAttributeSettings.sAMAccountName}={username}))";
                var searchRequest = new SearchRequest(
                    defaultNamingContext,
                    searchFilter,
                    SearchScope.Subtree,
                    LdapAttributeSettings.sn,
                    LdapAttributeSettings.givenName,
                    LdapAttributeSettings.mail,
                    LdapAttributeSettings.displayName
                );

                var searchResponse = (SearchResponse)connection.SendRequest(searchRequest);
                if (searchResponse.Entries.Count == 0)
                {
                    _logger.LogWarning("驗證成功，但找不到 AD 使用者詳細資料: {Username}", username);
                    return (false, null, "帳號或密碼錯誤");
                }

                var entry = searchResponse.Entries[0];
                var adUserInfo = new AdUserInfoDto
                {
                    Surname = entry.Attributes.Contains(LdapAttributeSettings.sn) ? entry.Attributes[LdapAttributeSettings.sn][0]?.ToString() : null,
                    GivenName = entry.Attributes.Contains(LdapAttributeSettings.givenName) ? entry.Attributes[LdapAttributeSettings.givenName][0]?.ToString() : null,
                    EmailAddress = entry.Attributes.Contains(LdapAttributeSettings.mail) ? entry.Attributes[LdapAttributeSettings.mail][0]?.ToString() : null,
                    DisplayName = entry.Attributes.Contains(LdapAttributeSettings.displayName) ? entry.Attributes[LdapAttributeSettings.displayName][0]?.ToString() : null
                };

                return (true, adUserInfo, null);
            }
            catch (LdapException ldapEx)
            {
                var detailedReason = ldapEx.ErrorCode switch
                {
                    49 => "帳號或密碼錯誤",
                    _ => $"AD 驗證失敗 (錯誤碼: {ldapEx.ErrorCode}): {ldapEx.Message}"
                };

                if (!string.IsNullOrEmpty(ldapEx.ServerErrorMessage))
                {
                    if (ldapEx.ServerErrorMessage.Contains("data 52e"))
                        detailedReason = "帳號或密碼錯誤";
                    else if (ldapEx.ServerErrorMessage.Contains("data 775"))
                        detailedReason = "帳號已被鎖定";
                    else if (ldapEx.ServerErrorMessage.Contains("data 533"))
                        detailedReason = "帳號已停用";
                    else if (ldapEx.ServerErrorMessage.Contains("data 532") || ldapEx.ServerErrorMessage.Contains("data 773"))
                        detailedReason = "密碼已過期或必須變更，請先變更密碼";
                    else if (ldapEx.ServerErrorMessage.Contains("data 701"))
                        detailedReason = "帳號已過期";
                    else
                        detailedReason = $"AD 驗證失敗: {ldapEx.ServerErrorMessage}";
                }

                _logger.LogWarning("AD 驗證失敗，帳號: {Username}，原因: {DetailedReason}，錯誤詳情: {ErrorMessage}", username, detailedReason, ldapEx.Message);
                return (false, null, "帳號或密碼錯誤");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("The server is unavailable", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError(ex, "AD 驗證失敗：伺服器無法使用，帳號: {Username}", username);
                }
                else
                {
                    _logger.LogError(ex, "AD 驗證發生非預期錯誤，帳號: {Username}", username);
                }
                return (false, null, "帳號或密碼錯誤");
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
