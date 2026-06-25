namespace DotNetApiTemplate.DTOs.Requests.User
{
    /// <summary>
    /// 更新使用者基本資料的請求 DTO
    /// </summary>
    public class UpdateUserRequest
    {
        public string? EmployeeName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
    }
}
