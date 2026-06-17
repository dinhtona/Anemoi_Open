using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Anemoi.Hr.Test.Helpers;

internal static class AsyncQueryableHelper
{
    public static IQueryable<T> CreateMockQueryable<T>(IEnumerable<T> items) where T : class
    {
        var queryable = items.AsQueryable();
        var provider = new TestAsyncQueryProvider<T>(queryable.Provider);
        return new TestAsyncQueryable<T>(provider, queryable.Expression);
    }
}

internal class TestAsyncQueryProvider<T>(IQueryProvider inner) : IAsyncQueryProvider
{
    private static readonly EfCoreExpressionCleaner Cleaner = new();

    public IQueryable CreateQuery(Expression expression)
    {
        var cleaned = Cleaner.Visit(expression);
        return new TestAsyncQueryable<T>(this, cleaned);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        var cleaned = Cleaner.Visit(expression);
        var provider = new TestAsyncQueryProvider<TElement>(inner);
        return new TestAsyncQueryable<TElement>(provider, cleaned);
    }

    public object Execute(Expression expression)
    {
        var cleaned = Cleaner.Visit(expression);
        return inner.Execute(cleaned);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        var cleaned = Cleaner.Visit(expression);
        return inner.Execute<TResult>(cleaned);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var cleaned = Cleaner.Visit(expression);
        var result = inner.Execute(cleaned);

        var taskType = typeof(TResult);
        if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var innerResultType = taskType.GetGenericArguments()[0];

            if (result is null && !innerResultType.IsValueType)
            {
                var fromResult = typeof(Task).GetMethod(nameof(Task.FromResult),
                    BindingFlags.Static | BindingFlags.Public)!.MakeGenericMethod(innerResultType);
                return (TResult)fromResult.Invoke(null, [null])!;
            }

            if (result is null && innerResultType.IsValueType)
            {
                var defaultValue = Activator.CreateInstance(innerResultType);
                var fromResult = typeof(Task).GetMethod(nameof(Task.FromResult),
                    BindingFlags.Static | BindingFlags.Public)!.MakeGenericMethod(innerResultType);
                return (TResult)fromResult.Invoke(null, [defaultValue])!;
            }

            var fromResultMethod = typeof(Task).GetMethod(nameof(Task.FromResult),
                BindingFlags.Static | BindingFlags.Public)!.MakeGenericMethod(innerResultType);
            return (TResult)fromResultMethod.Invoke(null, [result])!;
        }

        return (TResult)result;
    }
}

internal class TestAsyncQueryable<T>(IQueryProvider provider, Expression expression)
    : IQueryable<T>, IAsyncEnumerable<T>
{
    public IQueryProvider Provider => provider;
    public Expression Expression => expression;
    public Type ElementType => typeof(T);

    public IEnumerator<T> GetEnumerator()
    {
        var result = Provider.Execute<IEnumerable<T>>(Expression);
        return result.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new TestAsyncEnumerator<T>(GetEnumerator());
}

internal class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
{
    public T Current => inner.Current;
    public ValueTask<bool> MoveNextAsync() => new(inner.MoveNext());
    public ValueTask DisposeAsync() { inner.Dispose(); return new(); }
}

internal class EfCoreExpressionCleaner : ExpressionVisitor
{
    private static readonly HashSet<string> EfCoreAsyncMethods =
    [
        "FirstOrDefaultAsync", "FirstAsync", "SingleOrDefaultAsync",
        "SingleAsync", "CountAsync", "AnyAsync", "ToListAsync",
        "ToArrayAsync", "LastOrDefaultAsync", "LastAsync",
        "AllAsync", "ContainsAsync", "LongCountAsync",
        "MaxAsync", "MinAsync", "SumAsync", "AverageAsync",
        "ForEachAsync", "LoadAsync"
    ];

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method.DeclaringType == typeof(EntityFrameworkQueryableExtensions))
        {
            if (node.Method.Name is "Include" or "ThenInclude")
                return Visit(node.Arguments[0]);

            if (EfCoreAsyncMethods.Contains(node.Method.Name))
            {
                var sourceType = node.Method.GetGenericArguments()[0];
                var syncName = node.Method.Name.Replace("Async", "");
                var syncParamCount = node.Arguments.Count - 1;

                var syncMethod = typeof(Queryable)
                    .GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .FirstOrDefault(m =>
                    {
                        if (m.Name != syncName) return false;
                        var ps = m.GetParameters();
                        return ps.Length == syncParamCount
                            && ps[0].ParameterType.IsGenericType
                            && ps[0].ParameterType.GetGenericTypeDefinition() == typeof(IQueryable<>);
                    });

                if (syncMethod is not null)
                {
                    syncMethod = syncMethod.MakeGenericMethod(sourceType);
                    var sourceArgs = node.Arguments
                        .Take(syncParamCount)
                        .Select(Visit);
                    return Expression.Call(syncMethod, sourceArgs);
                }
            }
        }

        return base.VisitMethodCall(node);
    }
}
