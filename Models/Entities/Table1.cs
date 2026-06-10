using System.ComponentModel.DataAnnotations;

namespace DotNetApiTemplate.Models.Entities
{
    /// <summary>
    /// Table1 的基底類別
    /// </summary>
    public class Table1Base
    {
        public virtual int Table1Id { get; set; }

        public string? Column1 { get; set; }
    }

    /// <summary>
    /// Table1 的實體類別，繼承自 Table1Base
    /// </summary>
    public class Table1 : Table1Base
    {
        [Key]
        public override int Table1Id { get; set; }
    }
}


