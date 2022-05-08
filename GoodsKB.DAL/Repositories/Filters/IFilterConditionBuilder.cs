using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace GoodsKB.DAL.Repositories.Filters;

/// <summary>
/// Condition builder interface.
/// </summary>
/// <remarks>
/// Provides information about defined filters (<c>FilterAttribute</c>, <c>GroupFilterAttribute</c>)
/// for entity properties and functionality to build a conditional expression for a specified DTO
/// based on these custom filters that can be used in LINQ operations.
/// </remarks>
public interface IFilterConditionBuilder
{
	IReadOnlyDictionary<string, FilterDesc> GetFilters<TDto>() where TDto : notnull;

	[return: NotNullIfNotNull("values")]
	Expression? BuildCondition(FilterValues? values);
}