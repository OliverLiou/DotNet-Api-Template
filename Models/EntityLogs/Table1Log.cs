using System.ComponentModel.DataAnnotations;
using DotNetApiTemplate.Models.Entities;
using DotNetApiTemplate.Interfaces;

namespace DotNetApiTemplate.Models.EntityLogs
{
    /// <summary>
    /// Table1Log 的實體類別
    /// </summary>
    public class Table1Log : Table1Base, ILogInterface
    {
        [Key]
        public int Table1LogId { get; set; }

        public required string Method { get; set; }

        public required DateTime ExecuteTime { get; set; }

        public required string EditorName { get; set; }
    }
}


