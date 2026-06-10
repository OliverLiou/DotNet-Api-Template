using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using DotNetApiTemplate.Models.Context;
using DotNetApiTemplate.Interfaces;
using DotNetApiTemplate.Models.EntityLogs;

namespace DotNetApiTemplate.Services
{
    /// <summary>
    /// 泛型 Repository 實作
    /// </summary>
    public class RepositoryService<T, TLog>(TemplateContext context, IConfiguration configuration, IMapper mapper) : IRepositoryService<T, TLog> where T : class where TLog : class, ILogInterface
    {
        private static readonly MethodInfo StringContainsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
        private readonly TemplateContext _context = context;
        private readonly IMapper _mapper = mapper;
        private readonly string _create = configuration.GetSection("MethodName")["Create"]!;
        private readonly string _update = configuration.GetSection("MethodName")["Update"]!;
        private readonly string _delete = configuration.GetSection("MethodName")["Delete"]!;

        public async Task<T?> GetDataWithIdAsync(object[] id) => await _context.Set<T>().FindAsync(id);

        public async Task SaveSingleDataAsync(T entity, string editorName)
        {
            await using var _transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var id = GetPrimaryKeyValues(entity);
                var oldEntity = await GetDataWithIdAsync(id!);
                var methodName = _create;

                if (oldEntity == null)
                {
                    await _context.Set<T>().AddAsync(entity);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    methodName = _update;
                    _context.Entry(oldEntity).State = EntityState.Modified;
                    _context.Entry(oldEntity).CurrentValues.SetValues(entity);
                }

                var log = new Log() { Method = methodName, EditorName = editorName, ExecuteTime = DateTime.UtcNow };

                await ContextCreateLog(entity, log);

                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch (Exception)
            {
                await _transaction.RollbackAsync();
                throw;
            }
        }

        public async Task SaveMultipleDataAsync(List<T> entities, string editorName)
        {
            await using var _transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var utcNow = DateTime.UtcNow;
                var existingEntities = new List<(T CurrentEntity, T InputEntity)>();
                var newEntities = new List<T>();

                foreach (var entity in entities)
                {
                    var id = GetPrimaryKeyValues(entity);
                    var currentEntity = await GetDataWithIdAsync(id!);

                    if (currentEntity == null)
                        newEntities.Add(entity);
                    else
                        existingEntities.Add((currentEntity, entity));
                }

                foreach (var (currentEntity, inputEntity) in existingEntities)
                {
                    _context.Entry(currentEntity).CurrentValues.SetValues(inputEntity);
                    await ContextCreateLog(inputEntity, new Log { EditorName = editorName, Method = _update, ExecuteTime = utcNow });
                }

                if (newEntities.Count > 0)
                {
                    await _context.Set<T>().AddRangeAsync(newEntities);
                    //必須先儲存才能取得新增資料的Id，才能寫入Log
                    await _context.SaveChangesAsync();
                }

                foreach (var entity in newEntities)
                    await ContextCreateLog(entity, new Log { EditorName = editorName, Method = _create, ExecuteTime = utcNow });

                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch (Exception)
            {
                await _transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteSingleDataAsync(object[] id, string editorName)
        {
            await using var _transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var entity = await GetDataWithIdAsync(id);

                if (entity == null)
                    throw new Exception(string.Format("{0}中找不到Id為{1}的資料", typeof(T).Name, string.Join(",", id.Select(s => s))));

                var log = new Log() { EditorName = editorName, ExecuteTime = DateTime.UtcNow, Method = _delete };
                await ContextCreateLog(entity, log);

                _context.Entry(entity).State = EntityState.Deleted;

                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch (Exception)
            {
                await _transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<T>> GetAllDataAsync() => await _context.Set<T>().AsNoTracking().ToListAsync();

        public async Task<(List<T>, int)> FindDataAsync(int currentPage, int pageSize, string? querySearch, Expression<Func<T, bool>>? predicate, List<(string, bool)> sortColumns)
        {
            var items = _context.Set<T>().AsQueryable();

            if (predicate != null)
                items = items.Where(predicate);

            items = ApplyKeywordSearch(items, querySearch);

            var total = await items.CountAsync();

            if (sortColumns.Count > 0)
            {
                bool isFirst = true;
                foreach (var sortColumn in sortColumns)
                {
                    var (propertyName, isAscending) = sortColumn;
                    var parameter = Expression.Parameter(typeof(T), "x");
                    var property = Expression.Property(parameter, propertyName);
                    var lambda = Expression.Lambda(property, parameter);
                    var methodName = isFirst ? (isAscending ? "OrderBy" : "OrderByDescending") :
                                                (isAscending ? "ThenBy" : "ThenByDescending");

                    var genericMethod = typeof(Queryable).GetMethods().First(m => m.Name == methodName && m.GetParameters().Length == 2)
                                                         .MakeGenericMethod(typeof(T), property.Type);
                    items = (IQueryable<T>)genericMethod.Invoke(null, [items, lambda])!;
                    isFirst = false;
                }
            }

            items = items.Skip((currentPage - 1) * pageSize).Take(pageSize);

            return (await items.ToListAsync(), total);
        }

        private static IQueryable<T> ApplyKeywordSearch(IQueryable<T> items, string? querySearch)
        {
            if (string.IsNullOrWhiteSpace(querySearch))
                return items;

            var keyword = querySearch.Trim();
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? filterExpression = null;
            var includeNumericFields = keyword.All(char.IsDigit);

            foreach (var entityProperty in typeof(T).GetProperties())
            {
                var propertyExpression = BuildPropertyExpression(parameter, entityProperty, keyword, includeNumericFields);
                if (propertyExpression == null)
                    continue;

                filterExpression = filterExpression == null
                    ? propertyExpression
                    : Expression.OrElse(filterExpression, propertyExpression);
            }

            if (filterExpression == null)
                return items;

            var predicate = Expression.Lambda<Func<T, bool>>(filterExpression, parameter);
            return items.Where(predicate);
        }

        private static Expression? BuildPropertyExpression(ParameterExpression parameter, PropertyInfo entityProperty, string keyword, bool includeNumericFields)
        {
            var property = Expression.Property(parameter, entityProperty);

            if (entityProperty.PropertyType == typeof(string))
            {
                var notNullExpression = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));
                var containsExpression = Expression.Call(property, StringContainsMethod, Expression.Constant(keyword));
                return Expression.AndAlso(notNullExpression, containsExpression);
            }

            if (!includeNumericFields || !IsNumericType(entityProperty.PropertyType))
                return null;

            return BuildNumericContainsExpression(property, entityProperty.PropertyType, keyword);
        }

        private static Expression BuildNumericContainsExpression(MemberExpression property, Type propertyType, string keyword)
        {
            if (Nullable.GetUnderlyingType(propertyType) != null)
            {
                var hasValueExpression = Expression.Property(property, nameof(Nullable<int>.HasValue));
                var valueExpression = Expression.Property(property, nameof(Nullable<int>.Value));
                var containsExpression = BuildContainsExpression(valueExpression, valueExpression.Type, keyword);
                return Expression.AndAlso(hasValueExpression, containsExpression);
            }

            return BuildContainsExpression(property, propertyType, keyword);
        }

        private static Expression BuildContainsExpression(Expression property, Type propertyType, string keyword)
        {
            var toStringMethod = propertyType.GetMethod(nameof(object.ToString), Type.EmptyTypes)!;
            var stringValueExpression = Expression.Call(property, toStringMethod);
            return Expression.Call(stringValueExpression, StringContainsMethod, Expression.Constant(keyword));
        }

        private static bool IsNumericType(Type propertyType)
        {
            var actualType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            return actualType == typeof(byte)
                   || actualType == typeof(short)
                   || actualType == typeof(int)
                   || actualType == typeof(long)
                   || actualType == typeof(float)
                   || actualType == typeof(double)
                   || actualType == typeof(decimal);
        }

        public object?[] GetPrimaryKeyValues(T entity) => _context.Entry(entity).Metadata.FindPrimaryKey()!.Properties
                                                                    .Select(p => entity.GetType().GetProperty(p.Name)?.GetValue(entity))
                                                                    .ToArray();

        public async Task ContextCreateLog(T source, Log log)
        {
            var entityLog = _mapper.Map<TLog>(source);
            if (entityLog is ILogInterface logEntity)
            {
                logEntity.Method = log.Method;
                logEntity.ExecuteTime = log.ExecuteTime;
                logEntity.EditorName = log.EditorName;
            }

            await _context.Set<TLog>().AddAsync(entityLog);
        }
    }
}
