using System.Threading.Tasks;
using DotNetApiTemplate.Models.Entities;
using DotNetApiTemplate.DTOs.Responses.User;

namespace DotNetApiTemplate.Interfaces
{
    /// <summary>
    /// 統一商業邏輯層介面
    /// </summary>
    public interface ILogicService
    {
        /// <summary>
        /// AD 登入成功後，建立或更新使用者資料，並更新最後登入時間
        /// </summary>
        /// <param name="userName">使用者帳號</param>
        /// <param name="adUserInfo">AD 查詢到的使用者資訊</param>
        /// <param name="systemUserName">系統操作者名稱</param>
        /// <returns>已建立或更新的 User 實體</returns>
        Task<User> CreateOrUpdateUserOnLoginAsync(string userName, AdUserInfoDto adUserInfo, string systemUserName);
    }
}
