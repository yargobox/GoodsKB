using  GoodsKB.DAL.Repositories;

namespace GoodsKB.DAL.Entities;

public class ProdCat3 : IEntity<int?>, ISoftDelEntity<DateTimeOffset>
{
	public int? Id { get; set; }

	public string? Name { get; set; }
	public string? Desc { get; set; }

	public virtual IEnumerable<Product> Products { get; set; } = new List<Product>();

	public DateTimeOffset? Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }
}