namespace GoodsKB.DAL.Repositories.Filters;

[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public class FilterAttribute : Attribute
{
	FO? _allowed;
	FO? _default;
	bool? _isNullAllowed;
	bool? _isEmptyToNull;
	bool? _visible;

	public string? GroupFilter { get; set; }
	public bool IsNullAllowed { get => _isNullAllowed ?? false; set => _isNullAllowed = value; }
	public bool IsEmptyToNull { get => _isEmptyToNull ?? false; set => _isEmptyToNull = value; }
	public FO Default { get => _default ?? FO.None; set => _default = value; }
	public FO Allowed { get => _allowed ?? FO.None; set => _allowed = value; }
	public int Position { get; set; }
	public bool JoinByAnd { get; set; }
	public int ConditionOrder { get; set; } = 1000;
	public bool Visible { get => _visible ?? false; set => _visible = value; }

	public FilterAttribute()
		: this(null, 1000, false, null, null, null, null) { }
	public FilterAttribute(FO defaultOperation)
		: this(null, 1000, false, null, null, defaultOperation, null) { }
	public FilterAttribute(FO defaultOperation, FO allowed)
		: this(null, 1000, false, null, null, defaultOperation, allowed) { }
	public FilterAttribute(bool isNullAllowed, bool isEmptyToNull)
		: this(null, 1000, false, isNullAllowed, isEmptyToNull, null, null) { }
	public FilterAttribute(bool isNullAllowed, bool isEmptyToNull, FO defaultOperation)
		: this(null, 1000, false, isNullAllowed, isEmptyToNull, defaultOperation, null) { }
	public FilterAttribute(bool isNullAllowed, bool isEmptyToNull, FO defaultOperation, FO allowed)
		: this(null, 1000, false, isNullAllowed, isEmptyToNull, defaultOperation, allowed) { }
	public FilterAttribute(string groupFilter, int position = 1000, bool joinByAnd = false)
		: this(groupFilter, position, joinByAnd, null, null, null, null) { }
	public FilterAttribute(string groupFilter, FO defaultOperation, int position = 1000, bool joinByAnd = false)
		: this(groupFilter, position, joinByAnd, null, null, defaultOperation, null) { }
	public FilterAttribute(string groupFilter, FO defaultOperation, FO allowed, int position = 1000, bool joinByAnd = false)
		: this(groupFilter, position, joinByAnd, null, null, defaultOperation, allowed) { }
	public FilterAttribute(string groupFilter, bool isNullAllowed, bool isEmptyToNull, int position = 1000, bool joinByAnd = false)
		: this(groupFilter, position, joinByAnd, isNullAllowed, isEmptyToNull, null, null) { }
	public FilterAttribute(string groupFilter, bool isNullAllowed, bool isEmptyToNull, FO defaultOperation, int position = 1000, bool joinByAnd = false)
		: this(groupFilter, position, joinByAnd, isNullAllowed, isEmptyToNull, defaultOperation, null) { }
	public FilterAttribute(string groupFilter, bool isNullAllowed, bool isEmptyToNull, FO defaultOperation, FO allowed, int position = 1000, bool joinByAnd = false)
		: this(groupFilter, position, joinByAnd, isNullAllowed, isEmptyToNull, defaultOperation, allowed) { }
	protected FilterAttribute(string? groupFilter, int position, bool joinByAnd, bool? isNullAllowed, bool? isEmptyToNull, FO? defaultOperation, FO? allowed)
	{
		if (groupFilter != null && string.IsNullOrWhiteSpace(groupFilter))
			throw new ArgumentException(typeof(GroupFilterAttribute).Name + "." + nameof(GroupFilter));

		GroupFilter = groupFilter;
		_isNullAllowed = isNullAllowed;
		_isEmptyToNull = isEmptyToNull;
		_default = defaultOperation;
		_allowed = allowed;
		Position = position;
		JoinByAnd = joinByAnd;
	}

	public (bool? isNullAllowed, bool? isEmptyToNull, FO? defaultOperation, FO? allowed, bool? visible) GetInitialValues()
	{
		return (_isNullAllowed, _isEmptyToNull, _default, _allowed, _visible);
	}
}