namespace DotNetApiTemplate.DTOs.Interfaces
{
    public interface ILogInterface
    {
        string Method { get; set; }

        DateTime ExcuteTime { get; set; }

        string EditorName { get; set; }
    }
}
