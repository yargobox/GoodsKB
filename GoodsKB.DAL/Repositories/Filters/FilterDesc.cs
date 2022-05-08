using System.Diagnostics;

namespace GoodsKB.DAL.Repositories.Filters;

[DebuggerDisplay("{PropertyName,nq}: {Default.ToString(),nq} ({UnderlyingType.Name,nq}{IsNullAllowed?\"?\":\"\",nq})")]
public sealed record FilterDesc
{
	public record struct GroupFilterPartDesc(string Name, bool JoinByAnd) { }

	public FilterDesc(string propertyName, Type operandType, Type underlyingType)
	{
		PropertyName = propertyName;
		OperandType = operandType;
		UnderlyingType = underlyingType;
	}

	/// <summary>
	/// Entity property name
	/// </summary>
	public string PropertyName { get; init; }

	/// <summary>
	/// A data type of the filter operands that match the data type of the property.
	/// </summary>
	public Type OperandType { get; init; }

	/// <summary>
	/// Underlying system data type for the property (without Nullable struct)
	/// </summary>
	public Type UnderlyingType { get; init; }

	/// <summary>
	/// Allowed filter operations
	/// </summary>
	public FO Allowed { get; init; }

	/// <summary>
	/// Default filter operation
	/// </summary>
	public FO Default { get; init; }

	/// <summary>
	/// Whether arguments can be null.
	/// It also disables the TrueWhenNull operation flag in Allowed.
	/// </summary>
	public bool IsNullAllowed { get; init; }

	/// <summary>
	/// Applies to properties of the string data type. If the filter argument(s) is an empty string and
	/// the particular filter operation allows it, the argument(s) will automatically be converted to null.
	/// This property also depends on IsNullAllowed.
	/// </summary>
	public bool IsEmptyToNull { get; init; }

	/// <summary>
	/// Is the filter visible by default?
	/// </summary>
	public bool Visible { get; init; } = true;
/* 
	/// <summary>
	/// Filter position for UI
	/// </summary>
	public int Position { get; init; } = 0; */

	/// <summary>
	/// Is the filter allowed to be used in filter operations?
	/// </summary>
	/// <remarks>
	/// If a filter is defined through <c>FilterAttribute</c> without a group filter name,
	/// it can be used in filter operations. Otherwise, only the group filter can be used.
	/// </remarks>
	public bool Enabled { get; init; } = true;

	/// <summary>
	/// Is this a group filter?
	/// </summary>
	public bool IsGroupFilter => GroupParts != null;

	/// <summary>
	/// Enumeration of filters of the group filter in the specified order.
	/// </summary>
	public IEnumerable<GroupFilterPartDesc>? GroupParts { get; init; }
}