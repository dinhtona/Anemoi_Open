using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using OneOf;
using Anemoi.BuildingBlock.Domain.Models;

namespace Anemoi.BuildingBlock.Application.Abstractions;

public interface ISqlRepository<T> where T : class
{
    IQueryable<T> GetQueryable(Expression<Func<T, bool>> conditionExpression = null);

    /// <summary>
    /// SECURITY (TD-P34-SEC-03): this method passes <paramref name="sql"/> straight into EF Core's
    /// <c>FromSqlRaw</c>. Use ONLY for reviewed internal migrations/reports with fully static SQL —
    /// NEVER build the query from user input (SQL injection risk). Prefer LINQ-based queries instead.
    /// </summary>
    [Obsolete("Security: chỉ dùng cho migration/report nội bộ đã review với SQL tĩnh hoàn toàn — không nhận input từ user. Ưu tiên query LINQ thay thế.")]
    IQueryable<T> GetQueryableFromRawQuery(string sql, params object[] parameters);

    Task<T> GetFirstByConditionAsync(Expression<Func<T, bool>> conditionExpression = null,
        Func<IQueryable<T>, IQueryable<T>> specialAction = null, CancellationToken token = default);

    Task<bool> ExistByConditionAsync(Expression<Func<T, bool>> conditionExpression = null,
        CancellationToken token = default);

    Task<List<T>> GetManyByConditionAsync(Expression<Func<T, bool>> conditionExpression = null,
        Func<IQueryable<T>, IQueryable<T>> specialAction = null, CancellationToken token = default);

    Task<Pagination<T>> GetManyByConditionWithPaginationAsync(Expression<Func<T, bool>> conditionExpression = null,
        Func<IQueryable<T>, IQueryable<T>> specialAction = null, CancellationToken token = default);

    Task<long> CountByConditionAsync(Expression<Func<T, bool>> conditionExpression = null,
        Func<IQueryable<T>, IQueryable<T>> specialAction = null, CancellationToken token = default);

    Task<OneOf<T, Exception>> CreateOneAsync(T item, CancellationToken token = default);

    Task<OneOf<Results.None, Exception>> CreateManyAsync(List<T> items, CancellationToken token = default);

    Task<OneOf<Results.None, Exception>> RemoveOneAsync(OneOf<T, Expression<Func<T, bool>>> itemOrFilter,
        CancellationToken token = default);

    Task<OneOf<Results.None, Exception>> RemoveManyAsync(OneOf<List<T>, Expression<Func<T, bool>>> itemsOrFilter,
        CancellationToken token = default);
}