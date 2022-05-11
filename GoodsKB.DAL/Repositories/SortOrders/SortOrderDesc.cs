using System.Diagnostics;
using System.Reflection;

namespace GoodsKB.DAL.Repositories.SortOrders;

[DebuggerDisplay("{PropertyName,nq}: {Default.ToString(),nq}")]
public sealed record SortOrderDesc
{
	public SortOrderDesc(PropertyInfo propertyInfo, Type underlyingType)
	{
		PropertyInfo = propertyInfo;
		UnderlyingType = underlyingType;
	}

	/// <summary>
	/// Entity property info
	/// </summary>
	public PropertyInfo PropertyInfo { get; init; }

	/// <summary>
	/// Entity property name
	/// </summary>
	public string PropertyName => PropertyInfo.Name;

	/// <summary>
	/// Property data type
	/// </summary>
	public Type OperandType => PropertyInfo.PropertyType;

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