using  GoodsKB.DAL.Repositories;

namespace GoodsKB.DAL.Entities;

public class Direction : IEntityId<int>
{
	public int Id { get; set; }
	public void SetId(int id) => Id = id;
	public int GetId() => Id;

	public string? Name { get; set; }
	public string? Desc { get; set; }

	public virtual IEnumerable<ProdCat1> ProdCats1 { get; set; } = new List<ProdCat1>();

	public DateTimeOffset Created { get; set; }
	public DateTimeOffset Updated { get; set; }
	public DateTimeOffset Deleted { get; set; }
}