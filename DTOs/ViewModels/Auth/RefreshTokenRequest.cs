using System.ComponentModel.DataAnnotations;

namespace DotNetApiTemplate.DTOs.ViewModels.Auth
{
    /// <summary>
    /// RefreshTokenRequest 用於 RefreshToken 流程。
    /// </summary>
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "AccessToken 為必填欄位")]
        public string AccessToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "RefreshToken 為必填欄位")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
