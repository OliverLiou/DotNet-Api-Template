using System.ComponentModel.DataAnnotations;
using DotNetApiTemplate.DTOs.Entities;
using DotNetApiTemplate.DTOs.Interfaces;

namespace DotNetApiTemplate.DTOs.EntityLogs
{
    public class Table1Log : Table1Attribute, ILogInterface
    {
        [Key]
        public int Table1LogId { get; set; }

        public required string Method { get; set; }

        public required DateTime ExecuteTime { get; set; }

        public required string EditorName { get; set; }
    }
}
