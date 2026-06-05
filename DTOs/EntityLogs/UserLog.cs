using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotNetApiTemplate.DTOs.Entities;
using DotNetApiTemplate.DTOs.Interfaces;

namespace DotNetApiTemplate.DTOs.EntityLogs
{
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
