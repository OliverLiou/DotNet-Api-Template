using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using DotNetWebApiMssql.Interfaces;
using DotNetWebApiMssql.Models.Context;
using DotNetWebApiMssql.Models.Entities;
using DotNetWebApiMssql.Models.EntityLogs;
using DotNetWebApiMssql.DTOs.Responses.User;
using DotNetWebApiMssql.DTOs.Requests.User;

namespace DotNetWebApiMssql.Services
{
    /// <summary>
    /// 使用者業務邏輯服務實作
    /// </summary>
    public class UserService(
        IAuthService authService,
        IRepositoryService<User, UserLog> userRepository,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IRepositoryService<IdentityUserRole<string>, UserRoleLog> userRoleRepository,
        TemplateContext context) : IUserService
    {
        private readonly IAuthService _authService = authService;
        private readonly IRepositoryService<User, UserLog> _userRepository = userRepository;
        private readonly UserManager<User> _userManager = userManager;
        private readonly RoleManager<Role> _roleManager = roleManager;
        private readonly IRepositoryService<IdentityUserRole<string>, UserRoleLog> _userRoleRepository = userRoleRepository;
        private readonly TemplateContext _context = context;

        /// <summary>
        /// AD 登入成功後，建立或更新使用者資料，並更新最後登入時間
        /// </summary>
        public async Task<User> CreateOrUpdateUserOnLoginAsync(string userName, AdUserInfoDto adUserInfo, string systemUserName)
        {
            // 1. 查詢 DB 是否已有該使用者
            var user = await _authService.GetUserByUserNameAsync(userName);

            // 2. 如果沒有，建立新使用者實體 (使用對應方法)
            user ??= MapAdUserInfoToUser(userName, adUserInfo);

            // 3. 更新最後登入時間
            user.LastLoginAt = DateTime.UtcNow;

            // 4. 呼叫 Repository 儲存實體
            await _userRepository.SaveSingleDataAsync(user, systemUserName);

            return user;
        }

        /// <summary>
        /// 將 AD 使用者資訊 DTO 對應並轉換為系統的 User 實體
        /// </summary>
        public User MapAdUserInfoToUser(string userName, AdUserInfoDto adUserInfo)
        {
            return new User
            {
                UserName = userName,
                EmployeeName = (adUserInfo.Surname ?? null) + (adUserInfo.GivenName ?? null),
                Email = adUserInfo.EmailAddress ?? null,
                NormalizedUserName = userName.ToUpper(),
                NormalizedEmail = adUserInfo.EmailAddress?.ToUpper() ?? null,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
        }

        /// <summary>
        /// 更新使用者基本資料
        /// </summary>
        public async Task UpdateUserAsync(string userId, UpdateUserRequest request, string editorName)
        {
            var user = await _userRepository.GetDataWithIdAsync([userId])
                ?? throw new KeyNotFoundException("查無此使用者");

            user.EmployeeName = request.EmployeeName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;
            user.IsActive = request.IsActive;

            await _userRepository.SaveSingleDataAsync(user, editorName);
        }

        /// <summary>
        /// 更新使用者角色權限 (使用 RepositoryService 進行 Delete 與 Save Multiple 實作，並使用 Transaction 保證原子性)
        /// </summary>
        public async Task UpdateUserRolesAsync(string userId, List<string> roles, string editorName)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new KeyNotFoundException("查無此使用者");

            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToAddNames = roles.Except(currentRoles).ToList();
            var rolesToRemoveNames = currentRoles.Except(roles).ToList();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. 刪除角色：使用 DeleteSingleDataAsync
                foreach (var name in rolesToRemoveNames)
                {
                    var role = await _roleManager.FindByNameAsync(name);
                    if (role != null)
                    {
                        // 刪除關聯，主鍵為 [UserId, RoleId]
                        await _userRoleRepository.DeleteSingleDataAsync([userId, role.Id], editorName);
                    }
                }

                // 2. 新增角色：使用 SaveMultipleDataAsync
                var userRolesToAdd = new List<IdentityUserRole<string>>();
                foreach (var name in rolesToAddNames)
                {
                    var role = await _roleManager.FindByNameAsync(name);
                    if (role != null)
                    {
                        userRolesToAdd.Add(new IdentityUserRole<string>
                        {
                            UserId = userId,
                            RoleId = role.Id
                        });
                    }
                }

                if (userRolesToAdd.Count > 0)
                {
                    await _userRoleRepository.SaveMultipleDataAsync(userRolesToAdd, editorName);
                }

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// 更新使用者最後登入時間
        /// </summary>
        public async Task UpdateLastLoginTimeAsync(User user, string systemUserName)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.SaveSingleDataAsync(user, systemUserName);
        }
    }
}
