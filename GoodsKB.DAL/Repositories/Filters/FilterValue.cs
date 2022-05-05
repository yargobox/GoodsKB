namespace GoodsKB.DAL.Repositories.Filters;

/// <summary>
/// Filter operation context for a property
/// </summary>
public record struct FilterValue(string PropertyName)
{
	public FO Operation { get; init; } = FO.None;
	public object? Value { get; init; } = null;
	public object? Value2 { get; init; } = null;
}