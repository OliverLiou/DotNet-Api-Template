using System.ComponentModel.DataAnnotations;

namespace DotNetApiTemplate.ViewModels
{
    public class VGoogleLoginRequest
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }
}