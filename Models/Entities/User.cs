using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetApiTemplate.Models.Entities
{
    /// <summary>
    /// 使用者的基底類別，繼承自 IdentityUser
    /// </summary>
    public class UserBase : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// 使用者的實體類別，繼承自 UserBase
    /// </summary>
    public class User : UserBase
    {
        /// <summary>
        /// 員工名稱
        /// </summary>
        public string? EmployeeName { get; set; } = null;
    }
}


