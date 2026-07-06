using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotNetWebApiMssql.Models.Entities;
using DotNetWebApiMssql.Interfaces;

namespace DotNetWebApiMssql.Models.EntityLogs
{
    /// <summary>
    /// UserLog 的實體類別
    /// </summary>
    public class UserLog : UserBase, ILogInterface
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public required int UserLogId { get; set; }

        public required string Method { get; set; }

        public required DateTime ExecuteTime { get; set; }

        public required string EditorName { get; set; }
    }
}


