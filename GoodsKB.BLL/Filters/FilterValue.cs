using GoodsKB.DAL.Repositories;

namespace GoodsKB.BLL.Services;

public record struct FilterValue(string Name)
{
	public FilterOperations Operation { get; init; } = FilterOperations.None;
	public object? Value { get; init; } = null;
	public object? Value2 { get; init; } = null;
}