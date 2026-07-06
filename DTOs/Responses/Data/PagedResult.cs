namespace DotNetWebApiMssql.DTOs.Responses.Data
{
    /// <summary>
    /// 分頁結果的 ViewModel，包含資料列表和符合查詢條件的資料總筆數
    /// </summary>
    public class PagedResult<T>(List<T> items, int totalCount)
    {
        /// <summary>
        /// 分頁結果的資料列表
        /// </summary>
        public List<T> Items { get; } = items;

        /// <summary>
        /// 符合查詢條件的資料總筆數
        /// </summary>
        public int TotalCount { get; } = totalCount;
    }
}


