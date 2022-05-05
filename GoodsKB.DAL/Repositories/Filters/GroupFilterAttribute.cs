namespace GoodsKB.DAL.Repositories.Filters;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
public class GroupFilterAttribute : Attribute
{
	public string GroupFilter { get; set; }
	public int Position { get; set; }
	public bool JoinByAnd { get; set; }
	public int ConditionOrder { get; set; }
	public bool Visible { get; set; }

	public GroupFilterAttribute(string groupFilter, int position = 1000, bool joinByAnd = false, bool visible = true)
	{
		if (string.IsNullOrWhiteSpace(groupFilter))
			throw new ArgumentException(typeof(GroupFilterAttribute).Name + "." + nameof(GroupFilter));

		GroupFilter = groupFilter;
		Position = position;
		JoinByAnd = joinByAnd;
		Visible = visible;
	}
}