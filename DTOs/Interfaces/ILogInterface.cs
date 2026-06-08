namespace DotNetApiTemplate.DTOs.Interfaces
{
    public interface ILogInterface
    {
        string Method { get; set; }

        DateTime ExecuteTime { get; set; }

        string EditorName { get; set; }
    }
}
