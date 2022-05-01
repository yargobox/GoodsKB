using System.Linq.Expressions;
using GoodsKB.DAL.Repositories;

namespace GoodsKB.BLL.Services;

public sealed record FilterDefinition(string Name, Type OperandType, Type UnderlyingOperandType, Expression MemeberSelector)
{
	/// <summary>
	/// Allowed operations
	/// </summary>
	public FilterOperations Allowed { get; init; }
	/// <summary>
	/// Default operation
	/// </summary>
	public FilterOperations Default { get; init; }
	/// <summary>
	/// Whether arguments can be null.
	/// It also disables the TrueWhenNull operation flag in Allowed.
	/// </summary>
	public bool IsNullAllowed { get; init; }
	/// <summary>
	/// Applies to properties of the string data type.
	/// If the argument is an empty string, it will be automatically converted to null.
	/// This property also depends on IsNullAllowed.
	/// </summary>
	public bool EmptyToNull { get; init; }
}