using Microsoft.EntityFrameworkCore;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories.Filters;
using System.Reflection;

namespace O2Connect.Api.Repositories.DatabaseRepositories;

public static class IQueryableExtensions
{
    public static IQueryable<TEntity> ApplySorting<TEntity>(this IQueryable<TEntity> query,
                                                            EntityPagination pagination) 
        where TEntity : IIdentificable
    {
        var property = typeof(TEntity).GetProperty(pagination.SortBy,
                                                   BindingFlags.IgnoreCase
                                                   | BindingFlags.Public
                                                   | BindingFlags.Instance);

        if (property is null)
        {
            return query.OrderBy(i => i.Id);
        }

        return pagination.Order.Equals("desc", StringComparison.OrdinalIgnoreCase)
            ? query.OrderByDescending(e => EF.Property<object>(e!, property.Name))
            : query.OrderBy(e => EF.Property<object>(e!, property.Name));
    }

    public static IQueryable<TEntity> ApplyPagination<TEntity>(this IQueryable<TEntity> query,
                                                               EntityPagination pagination)
    {
        var skip = (pagination.Page - 1) * pagination.PageSize;

        return query.Skip(skip).Take(pagination.PageSize);
    }
}
