using DotNetApiTemplate.DTOs.Entities;
using DotNetApiTemplate.DTOs.ViewModels.User;

namespace DotNetApiTemplate.DTOs.Interfaces
{
    public interface IAuthService
    {
        Task<(bool IsValid, string? ErrorMessage)> AdAuthenticateAsync(string username, string password);
        Task<(AdUserInfoDto? AdUserInfo, string? ErrorMessage)> FetchAdUserPrincipal(string username);
        Task<User?> GetUserByUserNameAsync(string userName);
        Task<User?> GetUserByIdAsync(string userId);
        Task<IList<string>> GetUserRoleNamesAsync(User user);
    }
}
