namespace DotNetApiTemplate.DTOs.Responses.Data
{
    /// <summary>
    /// 使用者資料的回應 DTO
    /// </summary>
    public class UserResponse
    {
        public required string UserName { get; set; }
        public string? EmployeeName { get; set; }
        public string? Email { get; set; }
        public string? LastLoginAt { get; set; }
    }
}


