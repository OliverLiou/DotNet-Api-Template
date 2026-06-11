using System;
using System.Threading.Tasks;
using DotNetApiTemplate.Interfaces;
using DotNetApiTemplate.Models.Context;
using DotNetApiTemplate.Models.Entities;
using DotNetApiTemplate.Models.EntityLogs;
using DotNetApiTemplate.DTOs.Responses.User;

namespace DotNetApiTemplate.Services
{
    /// <summary>
    /// 統一商業邏輯層服務實作
    /// </summary>
    public class LogicService(
        IAuthService authService,
        IRepositoryService<User, UserLog> userRepository) : ILogicService
    {
        private readonly IAuthService _authService = authService;
        private readonly IRepositoryService<User, UserLog> _userRepository = userRepository;

        /// <summary>
        /// AD 登入成功後，建立或更新使用者資料，並更新最後登入時間
        /// </summary>
        public async Task<User> CreateOrUpdateUserOnLoginAsync(string userName, AdUserInfoDto adUserInfo, string systemUserName)
        {
            // 1. 查詢 DB 是否已有該使用者
            var user = await _authService.GetUserByUserNameAsync(userName);

            // 2. 如果沒有，建立新使用者實體
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

            // 3. 更新最後登入時間
            user.LastLoginAt = DateTime.UtcNow;

            // 4. 呼叫 Repository 儲存實體
            await _userRepository.SaveSingleDataAsync(user, systemUserName);

            return user;
        }
    }
}
