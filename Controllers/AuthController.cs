using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotNetApiTemplate.DTOs.ViewModels.Auth;
using DotNetApiTemplate.DTOs.ViewModels.User;
using DotNetApiTemplate.DTOs.Entities;
using DotNetApiTemplate.DTOs.EntityLogs;
using DotNetApiTemplate.DTOs.Settings;
using DotNetApiTemplate.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DotNetApiTemplate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IJwtService jwtService, IAuthService authService, IRepositoryService<User, UserLog> userRepositoryService, IConfiguration configuration, ILogger<AuthController> logger, IOptions<JwtSettings> jwtOptions) : ControllerBase
    {
        private readonly IJwtService _jwtService = jwtService;
        private readonly IAuthService _authService = authService;
        private readonly IRepositoryService<User, UserLog> _userRepositoryService = userRepositoryService;
        private readonly string _system_userName = configuration["SystemName"] ?? "System";
        private readonly ILogger<AuthController> _logger = logger;
        private readonly JwtSettings _jwtSettings = jwtOptions.Value;
        
        /// <summary>
        /// AD 登入流程：
        /// 1. 使用 DirectoryEntry 透過 LDAP Bind 驗證使用者帳號密碼。
        /// 2. 驗證成功後，從 AD 擷取使用者資料 (姓名、Email)。
        /// 3. 查詢 DB 是否已有該使用者，沒有則建立新使用者。
        /// 4. 生成 JWT token 
        /// </summary>
        [HttpPost("hcmf")]
        public async Task<ActionResult<AuthResponse>> AdLogin([FromBody] AdLoginRequest request)
        {
            try
            {
                var userName = request.UserName;
                var password = request.Password;

                // 透過 AD 驗證帳號密碼
                var (isValid, errorMessage) = await _authService.AdAuthenticateAsync(userName, password);

                if (!isValid)
                {
                    _logger.LogWarning("AD 登入驗證失敗: {UserName}, 原因: {Error}", userName, errorMessage);
                    return BadRequest(new AuthResponse { Success = false, Message = errorMessage ?? "驗證失敗" });
                }

                var (adUserInfo, fetchErrorMessage) = await _authService.FetchAdUserPrincipal(userName);

                if (adUserInfo == null)
                {
                    _logger.LogWarning("AD 使用者資料查詢失敗: {UserName}, 原因: {Error}", userName, fetchErrorMessage);
                    return BadRequest(new AuthResponse { Success = false, Message = fetchErrorMessage ?? "驗證失敗" });
                }

                // 查詢 DB 是否已有該使用者
                var user = await _authService.GetUserByUserNameAsync(userName);

                // 如果沒有，建立新使用者
                user ??= new User
                {
                    UserName = userName,
                    EmployeeName = (adUserInfo.Surname ?? null) + (adUserInfo.GivenName ?? null),
                    Email = adUserInfo.EmailAddress ?? null,
                    NormalizedUserName = userName.ToUpper(),
                    NormalizedEmail = adUserInfo.EmailAddress?.ToUpper() ?? null,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                user.LastLoginAt = DateTime.UtcNow;
                await _userRepositoryService.SaveSingleDataAsync(user, _system_userName);

                // 生成 JWT token
                var accessToken = _jwtService.GenerateToken(user, _jwtSettings.ExpiryInHours, JwtTokenTypes.Access);
                var refreshToken = _jwtService.GenerateToken(user, _jwtSettings.RefreshTokenExpiryInHours, JwtTokenTypes.Refresh);

                return Ok(new AuthResponse { Success = true, AccessToken = accessToken, RefreshToken = refreshToken });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AdLogin 發生未預期的錯誤");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "伺服器發生內部錯誤，請稍後再試"
                });
            }
        }


        /// <summary>
        /// 取得使用者個人資料，包含姓名、Email、角色等資訊
        /// </summary>
        [Authorize]
        [HttpGet("UserProfile")]
        public async Task<ActionResult<UserInfoDto>> UserProfile()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("UserProfile 無法從 token 取得使用者識別");
                    return Unauthorized();
                }

                var user = await _authService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("UserProfile 查無使用者: {UserId}", userId);
                    return NotFound();
                }

                var roleNames = await _authService.GetUserRoleNamesAsync(user);

                var userInfo = new UserInfoDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    EmployeeName = user.EmployeeName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    RoleNames = roleNames.ToList()
                };

                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserProfile 發生未預期的錯誤");
                return StatusCode(500, new { Message = "伺服器發生內部錯誤，請稍後再試" });
            }
        }

        
        /// <summary>
        /// 使用 refresh token 換發新的 access token, 沿用原 refresh token(不另行換發)
        /// </summary>
        [HttpPost("RefreshToken")]
        public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                ClaimsPrincipal refreshPrincipal;
                ClaimsPrincipal accessPrincipal;

                try
                {
                    refreshPrincipal = _jwtService.GetPrincipalFromToken(request.RefreshToken);
                    accessPrincipal = _jwtService.GetPrincipalFromToken(request.AccessToken, false);
                }
                catch (SecurityTokenException ex)
                {
                    _logger.LogWarning(ex, "RefreshToken 驗證失敗");
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = "Token 驗證失敗"
                    });
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning(ex, "RefreshToken 缺少必要 token");
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "AccessToken 與 RefreshToken 為必填欄位"
                    });
                }

                var refreshUserId = refreshPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var accessUserId = accessPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(refreshUserId) || string.IsNullOrWhiteSpace(accessUserId))
                {
                    _logger.LogWarning("RefreshToken 無法解析使用者識別");
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = "Token 內容無效"
                    });
                }

                if (!string.Equals(refreshUserId, accessUserId, StringComparison.Ordinal))
                {
                    _logger.LogWarning("RefreshToken 與 AccessToken 使用者不一致");
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = "AccessToken 與 RefreshToken 不一致"
                    });
                }

                var user = await _authService.GetUserByIdAsync(refreshUserId);

                if (user == null)
                {
                    _logger.LogWarning("RefreshToken 查無使用者: {UserId}", refreshUserId);
                    return NotFound(new AuthResponse
                    {
                        Success = false,
                        Message = "查無使用者"
                    });
                }

                var newAccessToken = _jwtService.GenerateToken(user, _jwtSettings.ExpiryInHours, JwtTokenTypes.Access);

                return Ok(new AuthResponse
                {
                    Success = true,
                    AccessToken = newAccessToken,
                    RefreshToken = request.RefreshToken
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RefreshToken 發生未預期的錯誤");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "伺服器發生內部錯誤，請稍後再試"
                });
            }
        }

    }
}
