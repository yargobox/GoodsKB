namespace GoodsKB.BLL.Services;

public enum SortOrders
{
	None = 0,
	Ascending = 1,
	Descending = 2
}

public record struct FieldSortOrderItem(string Name)
{
	public SortOrders SortOrder { get; init; } = SortOrders.None;
}

public interface IFieldSortOrderDefinition
{
	string Name { get; }
	Type OperandType { get; }
	FieldSortOrderItem DefaultValue { get; }
}