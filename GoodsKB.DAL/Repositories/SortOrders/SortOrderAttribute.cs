namespace GoodsKB.DAL.Repositories.SortOrders;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class SortOrderAttribute : Attribute
{
	protected SO? _allowed;
	protected SO? _default;

	public SO Default { get => _default ?? SO.None; init => _default = value; }
	public SO Allowed { get => _allowed ?? SO.None; init => _allowed = value; }

	public SortOrderAttribute()
		: this(null, null, null) { }
	public SortOrderAttribute(SO defaultOperation)
		: this(defaultOperation, null, null) { }
	public SortOrderAttribute(SO defaultOperation, SO allowed)
		: this(defaultOperation, allowed, null) { }
	protected SortOrderAttribute(SO? defaultOperation, SO? allowed, object? _)
	{
		_default = defaultOperation;
		_allowed = allowed;
	}

	public (SO? defaultOperation, SO? allowed) GetInitialValues()
	{
		return (_default, _allowed);
	}
}