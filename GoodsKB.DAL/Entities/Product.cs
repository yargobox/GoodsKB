using  GoodsKB.DAL.Repositories;

namespace GoodsKB.DAL.Entities;

public class Product : IIdentifiableEntity<int>
{
	public Product()
	{
		ProdCat1 = ProdCat1.Empty;
		ProdCat2 = ProdCat2.Empty;
		ProdCat3 = ProdCat3.Empty;
		Brand = Brand.Empty;
	}

	public int Id { get; set; }

	public string? Name { get; set; }
	public string? Desc { get; set; }

	public ProdCat1 ProdCat1 { get; set; }
	public ProdCat2 ProdCat2 { get; set; }
	public ProdCat3 ProdCat3 { get; set; }
	public Brand Brand { get; set; }

	public DateTimeOffset Created { get; set; }
	public DateTimeOffset Updated { get; set; }
	public DateTimeOffset Deleted { get; set; }
}