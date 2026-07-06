using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetWebApiMssql.Models.Entities;
using DotNetWebApiMssql.DTOs.Responses.User;
using DotNetWebApiMssql.DTOs.Requests.User;

namespace DotNetWebApiMssql.Interfaces
{
    /// <summary>
    /// 使用者業務邏輯服務介面
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// AD 登入成功後，建立或更新使用者資料，並更新最後登入時間
        /// </summary>
        /// <param name="userName">使用者帳號</param>
        /// <param name="adUserInfo">AD 查詢到的使用者資訊</param>
        /// <param name="systemUserName">系統操作者名稱</param>
        /// <returns>已建立或更新的 User 實體</returns>
        Task<User> CreateOrUpdateUserOnLoginAsync(string userName, AdUserInfoDto adUserInfo, string systemUserName);

        /// <summary>
        /// 更新使用者基本資料
        /// </summary>
        Task UpdateUserAsync(string userId, UpdateUserRequest request, string editorName);

        /// <summary>
        /// 更新使用者角色權限
        /// </summary>
        Task UpdateUserRolesAsync(string userId, List<string> roles, string editorName);

        /// <summary>
        /// 更新使用者最後登入時間
        /// </summary>
        Task UpdateLastLoginTimeAsync(User user, string systemUserName);

        /// <summary>
        /// 將 AD 使用者資訊 DTO 對應並轉換為系統的 User 實體
        /// </summary>
        /// <param name="userName">使用者帳號</param>
        /// <param name="adUserInfo">AD 使用者資訊 DTO</param>
        /// <returns>轉換後的 User 實體</returns>
        User MapAdUserInfoToUser(string userName, AdUserInfoDto adUserInfo);
    }
}
