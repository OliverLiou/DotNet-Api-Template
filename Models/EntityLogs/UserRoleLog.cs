using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotNetWebApiMssql.Interfaces;

namespace DotNetWebApiMssql.Models.EntityLogs
{
    /// <summary>
    /// UserRole 變更日誌實體
    /// </summary>
    public class UserRoleLog : ILogInterface
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserRoleLogId { get; set; }

        public required string UserId { get; set; }

        public required string RoleId { get; set; }

        public required string Method { get; set; }

        public required DateTime ExecuteTime { get; set; }

        public required string EditorName { get; set; }
    }
}
