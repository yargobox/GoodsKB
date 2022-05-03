namespace GoodsKB.DAL.Repositories.Filters;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
public class GroupFilterAttribute : Attribute
{
	public string Name { get; set; }
	public int Order { get; set; }
	public bool JoinByAnd { get; set; }

	public GroupFilterAttribute(string name, int order = 0, bool joinByAnd = false)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException(typeof(GroupFilterAttribute).Name + "." + nameof(Name));

		Name = name;
		Order = order;
		JoinByAnd = joinByAnd;
	}
}