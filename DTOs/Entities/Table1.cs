using System.ComponentModel.DataAnnotations;

namespace DotNetApiTemplate.DTOs.Entities
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
}
