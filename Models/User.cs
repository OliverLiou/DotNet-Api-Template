using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using  DotNetApiTemplate.Interface;

namespace DotNetApiTemplate.Models
{
    public class UserAttribute : IdentityUser
    {
        public string? GoogleId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

        public string? Picture { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class User : UserAttribute
    {
    }

    public class UserLog : UserAttribute, ILogInterface
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public required int UserLogId { get; set; }

        public required string Method { get; set; }

        public required DateTime ExcuteTime { get; set; }

        public required string EditorName { get; set; }
    }
}