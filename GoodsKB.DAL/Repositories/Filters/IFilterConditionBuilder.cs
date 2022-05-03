using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace GoodsKB.DAL.Repositories.Filters;

/// <summary>
/// Condition builder interface.
/// 
/// Provides information about defined filters (FilterAttribute) for entity properties and
/// functionality to build a conditional expression for this entity based on custom filters
/// that can be used in LINQ operations. 
/// </summary>
public interface IFilterConditionBuilder
{
	ReadOnlyDictionary<string, FilterDesc> GetFilters<TDto>();

	[return: NotNullIfNotNull("values")]
	Expression? BuildCondition(FilterValues? values);
}