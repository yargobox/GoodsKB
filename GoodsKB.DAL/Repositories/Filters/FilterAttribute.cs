namespace GoodsKB.DAL.Repositories.Filters;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class FilterAttribute : Attribute
{
	protected FO? _allowed;
	protected FO? _default;
	protected bool? _isNullAllowed;
	protected bool? _isEmptyToNull;
	protected bool? _visible;
	protected int? _position;

	public bool IsNullAllowed { get => _isNullAllowed ?? false; init => _isNullAllowed = value; }
	public bool IsEmptyToNull { get => _isEmptyToNull ?? false; init => _isEmptyToNull = value; }
	public FO Default { get => _default ?? FO.None; init => _default = value; }
	public FO Allowed { get => _allowed ?? FO.None; init => _allowed = value; }
	public int Position { get => _position ?? 1000; init => _position = value; }
	public bool Visible { get => _visible ?? false; init => _visible = value; }

	public FilterAttribute()
		: this(null, null, null, null, null, null) { }
	public FilterAttribute(int position)
		: this(position, null, null, null, null, null) { }
	public FilterAttribute(FO defaultOperation)
		: this(null, null, null, defaultOperation, null, null) { }
	public FilterAttribute(int position, FO defaultOperation)
		: this(position, null, null, defaultOperation, null, null) { }
	public FilterAttribute(FO defaultOperation, FO allowed)
		: this(null, null, null, defaultOperation, allowed, null) { }
	public FilterAttribute(int position, FO defaultOperation, FO allowed)
		: this(position, null, null, defaultOperation, allowed, null) { }
	public FilterAttribute(bool isNullAllowed, bool isEmptyToNull)
		: this(null, isNullAllowed, isEmptyToNull, null, null, null) { }
	public FilterAttribute(int position, bool isNullAllowed, bool isEmptyToNull)
		: this(position, isNullAllowed, isEmptyToNull, null, null, null) { }
	public FilterAttribute(bool isNullAllowed, bool isEmptyToNull, FO defaultOperation)
		: this(null, isNullAllowed, isEmptyToNull, defaultOperation, null, null) { }
	public FilterAttribute(int position, bool isNullAllowed, bool isEmptyToNull, FO defaultOperation)
		: this(position, isNullAllowed, isEmptyToNull, defaultOperation, null, null) { }
	public FilterAttribute(bool isNullAllowed, bool isEmptyToNull, FO defaultOperation, FO allowed)
		: this(null, isNullAllowed, isEmptyToNull, defaultOperation, allowed, null) { }
	public FilterAttribute(int position, bool isNullAllowed, bool isEmptyToNull, FO defaultOperation, FO allowed)
		: this(position, isNullAllowed, isEmptyToNull, defaultOperation, allowed, null) { }
	protected FilterAttribute(int? position, bool? isNullAllowed, bool? isEmptyToNull, FO? defaultOperation, FO? allowed, object? _)
	{
		_isNullAllowed = isNullAllowed;
		_isEmptyToNull = isEmptyToNull;
		_default = defaultOperation;
		_allowed = allowed;
		_position = position;
	}

	public (bool? isNullAllowed, bool? isEmptyToNull, FO? defaultOperation, FO? allowed, bool? visible, int? position) GetInitialValues()
	{
		return (_isNullAllowed, _isEmptyToNull, _default, _allowed, _visible, _position);
	}
}