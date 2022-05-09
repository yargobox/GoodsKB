using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace GoodsKB.DAL.Repositories.Filters;

/// <summary>
/// Filter conditions builder
/// </summary>
/// <remarks>
/// Provides information about defined filters (<c>FilterAttribute</c>, <c>GroupFilterAttribute</c>,
/// <c>FilterPartAttribute</c>) for DTO properties and functionality for building and applying
/// custom filtering conditions to an IQueryable instance based on them.
/// </remarks>
public interface IFilterConditionBuilder
{
	IReadOnlyDictionary<string, FilterDesc> GetFilters<TDto>() where TDto : notnull;

	T Apply<T>(T queryable, FilterValues? values) where T : IQueryable;

	[return: NotNullIfNotNull("values")]
	Expression? BuildCondition(FilterValues? values);
}