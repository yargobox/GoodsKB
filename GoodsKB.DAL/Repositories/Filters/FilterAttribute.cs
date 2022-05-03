namespace GoodsKB.DAL.Repositories.Filters;

[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public class FilterAttribute : Attribute
{
	FO? _allowed;
	FO? _default;
	bool? _isNullAllowed;
	bool? _isEmptyToNull;

	public string? Name { get; set; }
	public bool IsNullAllowed { get => _isNullAllowed ?? false; set => _isNullAllowed = value; }
	public bool IsEmptyToNull { get => _isEmptyToNull ?? false; set => _isEmptyToNull = value; }
	public FO Default { get => _default ?? FO.None; set => _default = value; }
	public FO Allowed { get => _allowed ?? FO.None; set => _allowed = value; }
	public int Order { get; set; }
	public bool JoinByAnd { get; set; }

	public FilterAttribute()
		: this(null, 0, false, null, null, null, null) { }
	public FilterAttribute(FO defaultOperation)
		: this(null, 0, false, null, null, defaultOperation, null) { }
	public FilterAttribute(FO defaultOperation, FO allowed)
		: this(null, 0, false, null, null, defaultOperation, allowed) { }
	public FilterAttribute(bool isNullAllowed, bool isEmptyToNull)
		: this(null, 0, false, isNullAllowed, isEmptyToNull, null, null) { }
	public FilterAttribute(bool isNullAllowed, bool isEmptyToNull, FO defaultOperation)
		: this(null, 0, false, isNullAllowed, isEmptyToNull, defaultOperation, null) { }
	public FilterAttribute(bool isNullAllowed, bool isEmptyToNull, FO defaultOperation, FO allowed)
		: this(null, 0, false, isNullAllowed, isEmptyToNull, defaultOperation, allowed) { }
	public FilterAttribute(string name, int order = 0, bool joinByAnd = false)
		: this(name, order, joinByAnd, null, null, null, null) { }
	public FilterAttribute(string name, FO defaultOperation, int order = 0, bool joinByAnd = false)
		: this(name, order, joinByAnd, null, null, defaultOperation, null) { }
	public FilterAttribute(string name, FO defaultOperation, FO allowed, int order = 0, bool joinByAnd = false)
		: this(name, order, joinByAnd, null, null, defaultOperation, allowed) { }
	public FilterAttribute(string name, bool isNullAllowed, bool isEmptyToNull, int order = 0, bool joinByAnd = false)
		: this(name, order, joinByAnd, isNullAllowed, isEmptyToNull, null, null) { }
	public FilterAttribute(string name, bool isNullAllowed, bool isEmptyToNull, FO defaultOperation, int order = 0, bool joinByAnd = false)
		: this(name, order, joinByAnd, isNullAllowed, isEmptyToNull, defaultOperation, null) { }
	public FilterAttribute(string name, bool isNullAllowed, bool isEmptyToNull, FO defaultOperation, FO allowed, int order = 0, bool joinByAnd = false)
		: this(name, order, joinByAnd, isNullAllowed, isEmptyToNull, defaultOperation, allowed) { }
	protected FilterAttribute(string? name, int order, bool joinByAnd, bool? isNullAllowed, bool? isEmptyToNull, FO? defaultOperation, FO? allowed)
	{
		if (name != null && string.IsNullOrWhiteSpace(name))
			throw new ArgumentException(typeof(GroupFilterAttribute).Name + "." + nameof(Name));

		Name = name;
		_isNullAllowed = isNullAllowed;
		_isEmptyToNull = isEmptyToNull;
		_default = defaultOperation;
		_allowed = allowed;
		Order = order;
		JoinByAnd = joinByAnd;
	}

	public (bool? isNullAllowed, bool? isEmptyToNull, FO? defaultOperation, FO? allowed) GetInitialValues()
	{
		return (_isNullAllowed, _isEmptyToNull, _default, _allowed);
	}
}