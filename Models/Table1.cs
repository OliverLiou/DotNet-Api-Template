using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using  DotNetApiTemplate.Interface;

namespace DotNetApiTemplate.Models
{
    public class Table1Attribute
    {
        public virtual int Table1Id { get; set; }

        public string? Column1 { get; set; }
    }

    public class Table1 : Table1Attribute
    {
        [Key]
        public override int Table1Id { get; set; }
    }

    public class Table1Log : Table1Attribute, ILogInterface
    {
        [Key]
        public int Table1LogId { get; set; }

        public required string Method { get; set; }

        public required DateTime ExcuteTime { get; set; }

        public required string EditorName { get; set; }
    }
}