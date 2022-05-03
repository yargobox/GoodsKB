namespace GoodsKB.DAL.Repositories.Filters;

public record struct FilterValue(string Name)
{
	public FO Operation { get; init; } = FO.None;
	public object? Value { get; init; } = null;
	public object? Value2 { get; init; } = null;
}