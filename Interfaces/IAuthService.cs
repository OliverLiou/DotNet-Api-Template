using DotNetApiTemplate.Models.Entities;
using DotNetApiTemplate.DTOs.Responses.User;

namespace DotNetApiTemplate.Interfaces
{
    public interface IAuthService
    {
        Task<(bool IsValid, string? ErrorMessage)> CheckAdAccountExistAsync(string username);
        Task<(bool IsValid, AdUserInfoDto? AdUserInfo, string? ErrorMessage)> AuthenticateAndFetchAdUserAsync(string username, string password);
        Task<(bool IsSuccess, User? User)> PasswordAuthenticateAsync(string username, string password);
        Task<User?> GetUserByUserNameAsync(string userName);
        Task<User?> GetUserByIdAsync(string userId);
        Task<IList<string>> GetUserRoleNamesAsync(User user);
    }
}


