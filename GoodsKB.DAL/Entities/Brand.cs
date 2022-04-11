using  GoodsKB.DAL.Repositories;

namespace GoodsKB.DAL.Entities;

public class Brand : IEntityId<int>
{
	public static readonly Brand Empty = new Brand();

	public int Id { get; set; }
	public void SetId(int id) => Id = id;
	public int GetId() => Id;

	public string? Name { get; set; }
	public string? Desc { get; set; }

	public DateTimeOffset Created { get; set; }
	public DateTimeOffset Updated { get; set; }
	public DateTimeOffset Deleted { get; set; }
}