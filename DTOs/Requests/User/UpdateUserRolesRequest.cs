using System.Collections.Generic;

namespace DotNetWebApiMssql.DTOs.Requests.User
{
    /// <summary>
    /// 更新使用者角色權限的請求 DTO
    /// </summary>
    public class UpdateUserRolesRequest
    {
        public List<string> Roles { get; set; } = new();
    }
}
