using System.ComponentModel.DataAnnotations;

namespace DotNetApiTemplate.DTOs.Requests.Auth
{
    public class AdLoginRequest
    {
        [Required(ErrorMessage = "帳號為必填欄位")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "密碼為必填欄位")]
        public string Password { get; set; } = string.Empty;
    }
}


