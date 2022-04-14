using  GoodsKB.DAL.Repositories;

namespace GoodsKB.DAL.Entities;

public class Brand : IIdentifiableEntity<int>, ISoftDelEntity<DateTimeOffset>
{
	public int Id { get; set; }

	public string? Name { get; set; }
	public string? Desc { get; set; }

	public DateTimeOffset Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }
}