namespace GoodsKB.DAL.Repositories.Filters;

[AttributeUsage(AttributeTargets.Property)]
public class FilterAttribute : Attribute
{
	FO? _allowed;
	FO? _default;
	bool? _isNullAllowed;
	bool? _isEmptyToNull;

	public bool IsNullAllowed { get { return _isNullAllowed ?? false; } set { _isNullAllowed = value; } }
	public bool IsEmptyToNull { get { return _isEmptyToNull ?? false; } set { _isEmptyToNull = value; } }
	public FO Default { get { return _default ?? FO.None; } set { _default = value; } }
	public FO Allowed { get { return _allowed ?? FO.None; } set { _allowed = value; } }

	public FilterAttribute()
	{
	}
	public FilterAttribute(FO defaultOperation)
	{
		Default = defaultOperation;
	}
	public FilterAttribute(FO defaultOperation, FO allowed)
	{
		Default = defaultOperation;
		Allowed = allowed;
	}
	public FilterAttribute(bool isNullAllowed, bool isEmptyToNull)
	{
		IsNullAllowed = isNullAllowed;
		IsEmptyToNull = isEmptyToNull;
	}
	public FilterAttribute(bool isNullAllowed, bool isEmptyToNull, FO defaultOperation)
	{
		IsNullAllowed = isNullAllowed;
		IsEmptyToNull = isEmptyToNull;
		Default = defaultOperation;
	}
	public FilterAttribute(bool isNullAllowed, bool isEmptyToNull, FO defaultOperation, FO allowed)
	{
		IsNullAllowed = isNullAllowed;
		IsEmptyToNull = isEmptyToNull;
		Default = defaultOperation;
		Allowed = allowed;
	}

	public (bool? isNullAllowed, bool? isEmptyToNull, FO? defaultOperation, FO? allowed) GetValues()
	{
		return (_isNullAllowed, _isEmptyToNull, _default, _allowed);
	}
	public void SetValues(bool? isNullAllowed, bool? isEmptyToNull, FO? defaultOperation, FO? allowed)
	{
		_isNullAllowed = isNullAllowed;
		_isEmptyToNull = IsEmptyToNull;
		_default = defaultOperation;
		_allowed = allowed;
	}
}