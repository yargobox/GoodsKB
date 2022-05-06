namespace GoodsKB.DAL.Repositories.Filters;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
public class FilterPartAttribute : Attribute
{
	public string GroupFilter { get; init; }
	public int ConditionOrder { get; init; }
	public bool JoinByAnd { get; init; }

	public FilterPartAttribute(string groupFilter, int conditionOrder = 1000, bool joinByAnd = false)
	{
		if (string.IsNullOrWhiteSpace(groupFilter))
			throw new ArgumentException(typeof(GroupFilterAttribute).Name + "." + nameof(GroupFilter));

		GroupFilter = groupFilter;
		ConditionOrder = conditionOrder;
		JoinByAnd = joinByAnd;
	}
}