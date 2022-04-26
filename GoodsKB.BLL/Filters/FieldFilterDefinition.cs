using System.Linq.Expressions;
using GoodsKB.DAL.Repositories;

namespace GoodsKB.BLL.Services;

public sealed record FieldFilterDefinition(string Name, Type OperandType, Expression MemeberSelector)
{
	public FilterOperations AllowedOperations { get; init; }
	public bool IsNullAllowed { get; init; }
	public bool EmptyToNull { get; init; }
	public FilterOperations DefaultOperation { get; init; }
}