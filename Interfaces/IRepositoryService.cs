using System.Linq.Expressions;

namespace DotNetApiTemplate.Interfaces
{
    /// <summary>
    /// 泛型 Repository 介面，約束提升到介面級別
    /// </summary>
    /// <typeparam name="T">實體類型</typeparam>
    /// <typeparam name="TLog">日誌實體類型</typeparam>
    /// <typeparam name="ILogInterface">日誌介面類型</typeparam>
    public interface IRepositoryService<T, TLog> where T : class where TLog : class, ILogInterface
    {
        /// <summary>
        /// 取得單筆資料
        /// </summary>
        Task<T?> GetDataWithIdAsync(object[] id);

        /// <summary>
        /// 在同一個交易中完成，新增或更新單筆資料，並記錄操作日誌。
        /// </summary>
        Task SaveSingleDataAsync(T entity, string editorName);

        /// <summary>
        /// 在同一個交易中完成，新增或更新多筆資料，並記錄操作日誌。
        /// excuteTime與editorName需要一致
        /// </summary>
        Task SaveMultipleDataAsync(List<T> entities, string editorName);

        /// <summary>
        /// 在同一個交易中完成，刪除單筆資料，並記錄操作日誌。
        /// </summary>
        Task DeleteSingleDataAsync(object[] id, string editorName);

        /// <summary>
        /// 取得整個Table資料
        /// </summary>
        Task<List<T>> GetAllDataAsync();

        /// <summary>
        /// 找出範圍內的資料, 可下條件式、一般搜尋、排序
        /// </summary>
        Task<(List<T>, int)> FindDataAsync(int currentPage, int pageSize, string? querySearch,
                                           Expression<Func<T, bool>>? predicate,
                                           List<(string, bool)> sortColumns);
    }
}


