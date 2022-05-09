namespace GoodsKB.DAL.Repositories.SortOrders;

/// <summary>
/// Sort operation context for a property
/// </summary>
public record struct SortOrderValue(string PropertyName)
{
	public SO Operation { get; init; } = SO.None;
}