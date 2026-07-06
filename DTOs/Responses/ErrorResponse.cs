namespace DotNetWebApiMssql.DTOs.Responses
{
    /// <summary>
    /// 統一錯誤回應格式
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// 給前端/使用者看的錯誤訊息
        /// </summary>
        public required string Message { get; set; }
    }
}
