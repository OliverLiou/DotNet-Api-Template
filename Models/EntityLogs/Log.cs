using DotNetApiTemplate.Interfaces;

namespace DotNetApiTemplate.Models.EntityLogs
{
    /// <summary>
    /// Log 的實體類別，實作自 ILogInterface
    /// </summary>
    public class Log : ILogInterface
    {
        public required string Method { get; set; }

        public required DateTime ExecuteTime { get; set; }

        public required string EditorName { get; set; }
    }
}


