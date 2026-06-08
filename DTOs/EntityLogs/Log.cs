using DotNetApiTemplate.DTOs.Interfaces;

namespace DotNetApiTemplate.DTOs.EntityLogs
{
    public class Log : ILogInterface
    {
        public required string Method { get; set; }

        public required DateTime ExecuteTime { get; set; }

        public required string EditorName { get; set; }
    }
}
