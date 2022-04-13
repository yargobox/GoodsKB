using  GoodsKB.DAL.Repositories;

namespace GoodsKB.DAL.Entities;

public class ProdCat3 : IIdentEntity<int>
{
	public static readonly ProdCat3 Empty = new ProdCat3();

	public int Id { get; set; }

	public string? Name { get; set; }
	public string? Desc { get; set; }

	public virtual IEnumerable<Product> Products { get; set; } = new List<Product>();

	public DateTimeOffset Created { get; set; }
	public DateTimeOffset Updated { get; set; }
	public DateTimeOffset Deleted { get; set; }
}