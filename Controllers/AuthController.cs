using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DotNetApiTemplate.ViewModels;
using DotNetApiTemplate.Services;
using DotNetApiTemplate.Models;

namespace DotNetApiTemplate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IGoogleAuthService googleAuthService, IJwtService jwtService, IAuthService authService) : ControllerBase
    {
        private readonly IGoogleAuthService _googleAuthService = googleAuthService;
        private readonly IJwtService _jwtService = jwtService;
        private readonly IAuthService _authService = authService;

        [HttpPost("google")]
        public async Task<ActionResult<VAuthResponse>> GoogleLogin([FromBody] VGoogleLoginRequest request)
        {
            try
            {
                // 驗證 Google ID Token
                var googleUser = await _googleAuthService.VerifyGoogleTokenAsync(request.IdToken);
                if (googleUser == null)
                {
                    return BadRequest(new VAuthResponse
                    {
                        Success = false,
                        Message = "Invalid Google token"
                    });
                }

                User user;
                // 檢查使用者是否已存在
                var existingUser = await _authService.CheckUserExistsAsync(googleUser.GoogleId);
                if (existingUser != null)
                {
                    // 更新現有使用者資訊
                    existingUser.UserName = googleUser.Name;
                    existingUser.Email = googleUser.Email;
                    existingUser.Picture = googleUser.Picture;
                    existingUser.LastLoginAt = DateTime.UtcNow;

                    user = await _authService.UpdateUserAsync(existingUser);
                }
                else
                {
                    // 建立新使用者
                    user = await _authService.CreateUserAsync(googleUser);
                }

                // 生成 JWT token
                var token = _jwtService.GenerateToken(user);

                return Ok(new VAuthResponse
                {
                    Success = true,
                    Message = "Login successful",
                    Token = token,
                    VUserInfo = new VUserInfo
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        Picture = user.Picture
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new VAuthResponse
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpPost("verify")]
        public IActionResult VerifyToken([FromBody] string token)
        {
            var isValid = _jwtService.ValidateToken(token);
            return Ok(new { isValid });
        }
    }
}