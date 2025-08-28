
namespace DotNetApiTemplate.ViewModels
{
    public class VAuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public VUserInfo? VUserInfo { get; set; }
    }
}