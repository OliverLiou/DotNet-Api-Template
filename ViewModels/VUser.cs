using System.Collections.Generic;

namespace DotNetApiTemplate.ViewModels
{
    public class VUser
    {
        public string? Id { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public string? Email { get; set; }
        public string? DepartmentId { get; set; }
        public string? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }

        public List<string>? RoleNames { get; set; }
    }
}