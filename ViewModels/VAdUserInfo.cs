namespace DotNetApiTemplate.ViewModels
{
    /// <summary>
    /// 從 AD (UserPrincipal) 擷取出的使用者資料 DTO，
    /// 避免將 UserPrincipal 物件直接傳出 Service 層。
    /// </summary>
    public class VAdUserInfo
    {
        public string? Surname { get; set; } = null;
        public string? GivenName { get; set; } = null;
        public string? EmailAddress { get; set; } = null;
        public string? DisplayName { get; set; } = null;
    }
}
