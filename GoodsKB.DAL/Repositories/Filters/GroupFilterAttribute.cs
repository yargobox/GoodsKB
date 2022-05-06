namespace GoodsKB.DAL.Repositories.Filters;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
public class GroupFilterAttribute : FilterAttribute
{
	public string GroupFilter { get; init; }
	public int ConditionOrder { get; init; } = 100;
	public bool JoinByAnd { get; init; }

	public GroupFilterAttribute(string groupFilter)
		: this(groupFilter, null, null, null, null, null, null) { }
	public GroupFilterAttribute(string groupFilter, int position)
		: this(groupFilter, position, null, null, null, null, null) { }
	public GroupFilterAttribute(string groupFilter, FO defaultOperation)
		: this(groupFilter, null, null, null, defaultOperation, null, null) { }
	public GroupFilterAttribute(string groupFilter, int position, FO defaultOperation)
		: this(groupFilter, position, null, null, defaultOperation, null, null) { }
	public GroupFilterAttribute(string groupFilter, FO defaultOperation, FO allowed)
		: this(groupFilter, null, null, null, defaultOperation, allowed, null) { }
	public GroupFilterAttribute(string groupFilter, int position, FO defaultOperation, FO allowed)
		: this(groupFilter, position, null, null, defaultOperation, allowed, null) { }
	public GroupFilterAttribute(string groupFilter, bool isNullAllowed, bool isEmptyToNull)
		: this(groupFilter, null, isNullAllowed, isEmptyToNull, null, null, null) { }
	public GroupFilterAttribute(string groupFilter, int position, bool isNullAllowed, bool isEmptyToNull)
		: this(groupFilter, position, isNullAllowed, isEmptyToNull, null, null, null) { }
	public GroupFilterAttribute(string groupFilter, bool isNullAllowed, bool isEmptyToNull, FO defaultOperation)
		: this(groupFilter, null, isNullAllowed, isEmptyToNull, defaultOperation, null, null) { }
	public GroupFilterAttribute(string groupFilter, int position, bool isNullAllowed, bool isEmptyToNull, FO defaultOperation)
		: this(groupFilter, position, isNullAllowed, isEmptyToNull, defaultOperation, null, null) { }
	public GroupFilterAttribute(string groupFilter, bool isNullAllowed, bool isEmptyToNull, FO defaultOperation, FO allowed)
		: this(groupFilter, null, isNullAllowed, isEmptyToNull, defaultOperation, allowed, null) { }
	public GroupFilterAttribute(string groupFilter, int position, bool isNullAllowed, bool isEmptyToNull, FO defaultOperation, FO allowed)
		: this(groupFilter, position, isNullAllowed, isEmptyToNull, defaultOperation, allowed, null) { }
	protected GroupFilterAttribute(string groupFilter, int? position, bool? isNullAllowed, bool? isEmptyToNull, FO? defaultOperation, FO? allowed, object? _)
		: base(position, isNullAllowed, isEmptyToNull, defaultOperation, allowed, null)
	{
		if (string.IsNullOrWhiteSpace(groupFilter))
			throw new ArgumentException(typeof(GroupFilterAttribute).Name + "." + nameof(GroupFilter));
		
		GroupFilter = groupFilter;
	}
}