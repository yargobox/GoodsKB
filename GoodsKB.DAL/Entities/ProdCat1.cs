using  GoodsKB.DAL.Repositories;

namespace GoodsKB.DAL.Entities;

public class ProdCat1 : IUpdatedEntity<int?, DateTimeOffset>, ISoftDelEntity<DateTimeOffset>
{
	public int? Id { get; set; }

	public string? Name { get; set; }
	public string? Desc { get; set; }

	public virtual IEnumerable<ProdCat2> ProdCats2 { get; set; } = new List<ProdCat2>();

	public DateTimeOffset? Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }
}