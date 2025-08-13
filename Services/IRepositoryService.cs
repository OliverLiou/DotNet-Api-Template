using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using  DotNetApiTemplate;
using  DotNetApiTemplate.Interface;
using  DotNetApiTemplate.Models;

namespace DotNetApiTemplate.Services
{
    /// <summary>
    /// 泛型 Repository 介面，約束提升到介面級別
    /// </summary>
    /// <typeparam name="T">實體類型</typeparam>
    /// <typeparam name="TLog">日誌實體類型</typeparam>
    public interface IRepositoryService<T, TLog> where T : class where TLog : class
    {
        /// <summary>
        /// 取得單筆資料
        /// </summary>
        Task<T?> GetDataWithIdAsync(object[] id);

        /// <summary>
        /// 新增或更新單筆資料
        /// </summary>
        Task SaveSingleDataAsync(T entity, string editorName);

        /// <summary>
        /// 新增或更新多筆資料
        /// </summary>
        Task SaveMutipleDataAsync(List<T> entitys, string editorName);

        /// <summary>
        /// 刪除單筆資料
        /// </summary>
        Task DeleteSigleDataAsync(object[] id, string editorName);

        /// <summary>
        /// 取得整個Table資料
        /// </summary>
        Task<List<T>> GetAllDataAsync();

        /// <summary>
        /// 找出範圍內的資料, 可下條件式、排序
        /// </summary>
        Task<Tuple<List<T>, int>> FindDataAsync(int currentPage, int pageSize, string? querySearch,
                                                Expression<Func<T, bool>>? predicate,
                                                List<(string, bool)> sortColumns);
    }

    /// <summary>
    /// 泛型 Repository 實作
    /// </summary>
    public class RepositoryService<T, TLog>(TemplateContext context, IConfiguration configuration, IMapper mapper) : IRepositoryService<T, TLog> where T : class where TLog : class
    {
        private readonly TemplateContext _context = context;
        private readonly IMapper _mapper = mapper;
        private readonly string _create = configuration.GetSection("MethodName")["Create"]!;
        private readonly string _update = configuration.GetSection("MethodName")["Update"]!;
        private readonly string _delete = configuration.GetSection("MethodName")["Delete"]!;

        public async Task<T?> GetDataWithIdAsync(object[] id)
        {
            try
            {
                var result = await _context.Set<T>().FindAsync(id);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task SaveSingleDataAsync(T entity, string editorName)
        {
            try
            {
                var id = GetPrimaryKeyValues(entity);
                var oldEntity = await GetDataWithIdAsync(id!);
                var methodName = _create;

                if (oldEntity == null)
                    await _context.Set<T>().AddAsync(entity);
                else
                {
                    methodName = _update;
                    _context.Entry(oldEntity).State = EntityState.Modified;
                    _context.Entry(oldEntity).CurrentValues.SetValues(entity);
                }

                await _context.SaveChangesAsync();

                var log = new Log() { Method = methodName, EditorName = editorName, ExcuteTime = DateTime.Now };

                await ContextCreateLog(entity, log);

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task SaveMutipleDataAsync(List<T> entitys, string editorName)
        {
            try
            {
                var log = new Log() { EditorName = editorName, Method = "", ExcuteTime = DateTime.Now };

                foreach (var entity in entitys)
                {
                    var id = GetPrimaryKeyValues(entity);
                    var oldEntity = await GetDataWithIdAsync(id!);
                    if (oldEntity == null)
                    {
                        await _context.Set<T>().AddAsync(entity);
                        log.Method = _create;
                    }
                    else
                    {
                        _context.Entry(oldEntity).State = EntityState.Modified;
                        _context.Entry(oldEntity).CurrentValues.SetValues(entity);
                        log.Method = _update;
                    }

                    await _context.SaveChangesAsync();

                    await ContextCreateLog(entity, log);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeleteSigleDataAsync(object[] id, string editorName)
        {
            try
            {
                var entity = await GetDataWithIdAsync(id);

                if (entity == null)
                    throw new Exception(string.Format("{0}中找不到Id為{1}的資料", typeof(T).Name, string.Join(",", id.Select(s => s))));

                _context.Entry(entity).State = EntityState.Deleted;

                var log = new Log() { EditorName = editorName, ExcuteTime = DateTime.Now, Method = _delete };

                await ContextCreateLog(entity, log);

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<T>> GetAllDataAsync()
        {
            try
            {
                var items = await _context.Set<T>().ToListAsync();
                return items;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Tuple<List<T>, int>> FindDataAsync(int currentPage, int pageSize, string? querySearch,
                                                            Expression<Func<T, bool>>? predicate,
                                                            List<(string, bool)>? sortColumns)
        {
            try
            {
                var items = _context.Set<T>().AsQueryable();

                if (predicate != null)
                    items = items.Where(predicate);

                if (!string.IsNullOrEmpty(querySearch))
                    items = PublicMethod.setWhereStr(querySearch, typeof(T).GetProperties(), items);

                var total = await items.CountAsync();

                if (sortColumns != null)
                {
                    foreach (var sortColumn in sortColumns)
                    {
                        var (propertyName, isAscending) = sortColumn;
                        var parameter = Expression.Parameter(typeof(T), "x");
                        var property = Expression.Property(parameter, propertyName);
                        var lambda = Expression.Lambda(property, parameter);
                        var methodName = isAscending ? "OrderBy" : "OrderByDescending";

                        var genericMethod = typeof(Queryable).GetMethods().First(m => m.Name == methodName && m.GetParameters().Length == 2)
                                                             .MakeGenericMethod(typeof(T), property.Type);
                        items = (IQueryable<T>)genericMethod.Invoke(null, [items, lambda])!;
                    }
                }

                items = items.Skip((currentPage - 1) * pageSize).Take(pageSize);

                return Tuple.Create(await items.ToListAsync(), total);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public object?[] GetPrimaryKeyValues(T entity)
        {
            try
            {
                return _context.Entry(entity).Metadata.FindPrimaryKey()!.Properties
                               .Select(p => entity.GetType().GetProperty(p.Name)?.GetValue(entity))
                               .ToArray();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task ContextCreateLog(T source, Log log)
        {
            try
            {
                var entityLog = _mapper.Map<TLog>(source);
                var entityLogPropertys = typeof(TLog).GetProperties();

                var logPropertys = log.GetType().GetProperties();

                foreach (var logProperty in logPropertys)
                {
                    var name = logProperty.Name;
                    var entityLogProperty = entityLogPropertys.First(e => e.Name == name);
                    if (entityLogProperty != null && entityLogProperty.CanWrite)
                    {
                        entityLogProperty.SetValue(entityLog, logProperty.GetValue(log));
                    }
                }

                await _context.Set<TLog>().AddAsync(entityLog);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}