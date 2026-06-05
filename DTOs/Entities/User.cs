using Microsoft.AspNetCore.Identity;

namespace DotNetApiTemplate.DTOs.Entities
{
    public class UserAttribute : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }

    public class User : UserAttribute
    {
        /// <summary>
        /// 員工名稱
        /// </summary>
        public string? EmployeeName { get; set; } = null;
    }
}
