namespace GoodsKB.DAL.Entities;

using GoodsKB.DAL.Repositories;

public class Brand : IEntity<int?>, ISoftDelEntity<DateTimeOffset>
{
	public int? Id { get; set; }

	public string? Name { get; set; }

	public string? Desc { get; set; }

	public DateTimeOffset? Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }
}