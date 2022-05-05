using System.Collections.ObjectModel;

namespace GoodsKB.DAL.Repositories.Filters;

/// <summary>
/// Information sufficient to build a condition expression for a filtering operation
/// </summary>
public sealed class FilterValues
{
	public FilterValues(ReadOnlyDictionary<string, FilterDesc> filters, IEnumerable<FilterValue> values)
	{
		Filters = filters;
		Values = values;
	}

	public ReadOnlyDictionary<string, FilterDesc> Filters { get; init; }
	
	public IEnumerable<FilterValue> Values { get; init; }
}