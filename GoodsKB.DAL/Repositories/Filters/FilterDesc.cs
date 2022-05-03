using System.Linq.Expressions;

namespace GoodsKB.DAL.Repositories.Filters;

public sealed record FilterDesc
{
	public FilterDesc(string name, Type operandType, Type underlyingType, Expression memeberSelector)
	{
		Name = name;
		OperandType = operandType;
		UnderlyingType = underlyingType;
		MemeberSelector = memeberSelector;
	}

	/// <summary>
	/// Entity property name
	/// </summary>
	public string Name { get; init; }

	/// <summary>
	/// A data type of the filter operands that match the data type of the property.
	/// </summary>
	public Type OperandType { get; init; }

	/// <summary>
	/// Underlying system data type for the property (without Nullable struct)
	/// </summary>
	public Type UnderlyingType { get; init; }

	/// <summary>
	/// Entity property selector
	/// </summary>
	public Expression MemeberSelector { get; init; }

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
}