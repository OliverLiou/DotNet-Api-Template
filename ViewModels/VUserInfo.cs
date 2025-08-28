using System.Collections.Generic;

namespace DotNetApiTemplate.ViewModels
{
    public class VUserInfo
    {
        public string? Id { get; set; }
        public required string UserName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Picture { get; set; }
        
        public List<string>? RoleNames { get; set; }
    }
}