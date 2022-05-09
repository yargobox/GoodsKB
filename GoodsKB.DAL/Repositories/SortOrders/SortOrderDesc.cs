using System.Diagnostics;

namespace GoodsKB.DAL.Repositories.SortOrders;

[DebuggerDisplay("{PropertyName,nq}: {Default.ToString(),nq}")]
public sealed record SortOrderDesc
{
	public SortOrderDesc(string propertyName, Type operandType, Type underlyingType)
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
	/// Property data type
	/// </summary>
	public Type OperandType { get; init; }

	/// <summary>
	/// Underlying system data type for the property (without Nullable struct)
	/// </summary>
	public Type UnderlyingType { get; init; }

	/// <summary>
	/// Allowed sort operations
	/// </summary>
	public SO Allowed { get; init; }

	/// <summary>
	/// Default sort operation
	/// </summary>
	public SO Default { get; init; }
}