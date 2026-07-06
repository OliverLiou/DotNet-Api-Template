using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace DotNetWebApiMssql.Models.Entities
{
    /// <summary>
    /// 角色資料表
    /// </summary>
    public class Role : IdentityRole
    {
        /// <summary>
        /// 角色名稱
        /// </summary>
        [MaxLength(50)]
        public required string RoleDesc { get; set; }
    }
}


