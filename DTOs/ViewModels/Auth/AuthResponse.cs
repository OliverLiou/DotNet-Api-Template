using DotNetApiTemplate.DTOs.ViewModels.User;

namespace DotNetApiTemplate.DTOs.ViewModels.Auth
{
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AccessToken { get; set; } = null;
        public string? RefreshToken { get; set; } = null;
    }
}
