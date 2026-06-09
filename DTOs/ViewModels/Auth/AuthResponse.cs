using DotNetApiTemplate.DTOs.ViewModels.User;

namespace DotNetApiTemplate.DTOs.ViewModels.Auth
{
    public class AuthResponse
    {
        public string? AccessToken { get; set; } = null;
        public string? RefreshToken { get; set; } = null;
        // public string ErrorMessage { get; set; } = string.Empty;
    }
}
