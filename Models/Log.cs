using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using  DotNetApiTemplate.Interface;

namespace DotNetApiTemplate.Models
{
    public class Log: ILogInterface
    {
        public required string Method { get; set; }

        public required DateTime ExcuteTime { get; set; }

        public required string EditorName { get; set; }
    }
}