using GoodsKB.DAL.Repositories;

namespace GoodsKB.BLL.Services;

public record struct FieldFilterItem(string Name)
{
	public FilterOperations Operation { get; init; } = FilterOperations.None;
	public object? Value { get; init; } = null;
}

public sealed record FieldFilterDefinition(string Name, Type OperandType)
{
	public FilterOperations AllowedOperations { get; }
	public bool EmptyToNull { get; }
	public bool IsNullAllowed { get; }
	public FieldFilterItem DefaultValue { get; }
}