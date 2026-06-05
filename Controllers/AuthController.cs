using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DotNetApiTemplate.DTOs.ViewModels.Auth;
using DotNetApiTemplate.DTOs.ViewModels.User;
using DotNetApiTemplate.DTOs.Entities;
using DotNetApiTemplate.DTOs.EntityLogs;
using DotNetApiTemplate.Services;
using Microsoft.AspNetCore.Identity;

namespace DotNetApiTemplate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IJwtService jwtService, IAuthService authService, IRepositoryService<User, UserLog> userRepositoryService, IConfiguration configuration, ILogger<AuthController> logger) : ControllerBase
    {
        private readonly IJwtService _jwtService = jwtService;
        private readonly IAuthService _authService = authService;
        private readonly IRepositoryService<User, UserLog> _userRepositoryService = userRepositoryService;
        private readonly string _system_userName = configuration["SystemName"] ?? "System";
        private readonly ILogger<AuthController> _logger = logger;
        
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
                var accessToken = _jwtService.GenerateToken(user);

                return Ok(new AuthResponse { Success = true, AccessToken = accessToken });
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

    }
}
