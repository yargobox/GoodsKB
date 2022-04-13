using  GoodsKB.DAL.Repositories;

namespace GoodsKB.DAL.Entities;

public class ProdCat1 : IIdentEntity<int>
{
	public static readonly ProdCat1 Empty = new ProdCat1();

	public int Id { get; set; }

	public string? Name { get; set; }
	public string? Desc { get; set; }

	public virtual IEnumerable<ProdCat2> ProdCats2 { get; set; } = new List<ProdCat2>();

	public DateTimeOffset Created { get; set; }
	public DateTimeOffset Updated { get; set; }
	public DateTimeOffset Deleted { get; set; }
}