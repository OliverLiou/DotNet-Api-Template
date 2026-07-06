namespace DotNetWebApiMssql.DTOs.Requests.Auth
{
    /// <summary>
    /// 一般登入請求 DTO
    /// </summary>
    public class LoginRequest
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }
}
