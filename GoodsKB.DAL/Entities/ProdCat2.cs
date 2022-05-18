using  GoodsKB.DAL.Repositories;

namespace GoodsKB.DAL.Entities;

public class ProdCat2 : IUpdatedEntity<int?, DateTimeOffset>, ISoftDelEntity<DateTimeOffset>
{
	public int? Id { get; set; }

	public string? Name { get; set; }
	public string? Desc { get; set; }

	public virtual IEnumerable<ProdCat3> ProdCats3 { get; set; } = new List<ProdCat3>();

	public DateTimeOffset? Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }
}