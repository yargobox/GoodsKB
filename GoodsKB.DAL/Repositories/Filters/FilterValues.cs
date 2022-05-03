using System.Collections.ObjectModel;

namespace GoodsKB.DAL.Repositories.Filters;

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