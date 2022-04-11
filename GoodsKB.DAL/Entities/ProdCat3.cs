using  GoodsKB.DAL.Repositories;

namespace GoodsKB.DAL.Entities;

public class ProdCat3 : IEntityId<int>
{
	public static readonly ProdCat3 Empty = new ProdCat3();

	public int Id { get; set; }
	public void SetId(int id) => Id = id;
	public int GetId() => Id;

	public string? Name { get; set; }
	public string? Desc { get; set; }

	public virtual IEnumerable<Product> Products { get; set; } = new List<Product>();

	public DateTimeOffset Created { get; set; }
	public DateTimeOffset Updated { get; set; }
	public DateTimeOffset Deleted { get; set; }
}