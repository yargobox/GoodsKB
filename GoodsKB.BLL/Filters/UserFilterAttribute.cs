using GoodsKB.DAL.Repositories;

namespace GoodsKB.BLL.Services;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class UserFilterAttribute : Attribute
{
	bool? _isNullAllowed;
	bool? _isEmptyToNull;
	FilterOperations? _default;
	FilterOperations? _allowed;

	public bool IsNullAllowed { get { return _isNullAllowed ?? false; } set { _isNullAllowed = value; } }
	public bool IsEmptyToNull { get { return _isEmptyToNull ?? false; } set { _isEmptyToNull = value; } }
	public FilterOperations Default { get { return _default ?? FilterOperations.None; } set { _default = value; } }
	public FilterOperations Allowed { get { return _allowed ?? FilterOperations.None; } set { _allowed = value; } }

	public UserFilterAttribute()
	{
	}
	public UserFilterAttribute(bool isNullAllowed, bool emptyToNull)
	{
		IsNullAllowed = isNullAllowed;
		IsEmptyToNull = emptyToNull;
	}
	public UserFilterAttribute(FilterOperations defaultOperation)
	{
		Default = defaultOperation;
	}
	public UserFilterAttribute(FilterOperations defaultOperation, FilterOperations allowed)
	{
		Default = defaultOperation;
		Allowed = allowed;
	}
	public UserFilterAttribute(bool isNullAllowed, bool emptyToNull, FilterOperations defaultOperation)
	{
		IsNullAllowed = isNullAllowed;
		IsEmptyToNull = emptyToNull;
		Default = defaultOperation;
	}
	public UserFilterAttribute(bool isNullAllowed, bool emptyToNull, FilterOperations defaultOperation, FilterOperations allowed)
	{
		IsNullAllowed = isNullAllowed;
		IsEmptyToNull = emptyToNull;
		Default = defaultOperation;
		Allowed = allowed;
	}

	public (bool? isNullAllowed, bool? emptyToNull, FilterOperations? defaultOperation, FilterOperations? allowed) GetInitialValues()
	{
		return (_isNullAllowed, _isEmptyToNull, _default, _allowed);
	}
	public void SetInitialValues(bool? isNullAllowed, bool? emptyToNull, FilterOperations? defaultOperation, FilterOperations? allowed)
	{
		_isNullAllowed = isNullAllowed;
		_isEmptyToNull = IsEmptyToNull;
		_default = defaultOperation;
		_allowed = allowed;
	}
}