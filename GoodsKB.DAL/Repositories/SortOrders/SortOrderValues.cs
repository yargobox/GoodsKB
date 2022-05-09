namespace GoodsKB.DAL.Repositories.SortOrders;

/// <summary>
/// Information sufficient to build a conditional expression for a sorting operation
/// </summary>
public sealed class SortOrderValues
{
	public SortOrderValues(IReadOnlyDictionary<string, SortOrderDesc> sortOrders, IEnumerable<SortOrderValue> values)
	{
		SortOrders = sortOrders;
		Values = values;
	}

	public IReadOnlyDictionary<string, SortOrderDesc> SortOrders { get; init; }
	
	public IEnumerable<SortOrderValue> Values { get; init; }
}