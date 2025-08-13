using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetApiTemplate.Interface
{
    public interface ILogInterface
    {
        string Method { get; set; }

        DateTime ExcuteTime { get; set; }

        string EditorName { get; set; }
    }
}