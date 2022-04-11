using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;

using GoodsKB.DAL.Repositories;

namespace GoodsKB.DAL;

public interface IDALContext
{
	IBaseRepo<int, User> Users { get; }
	IBaseRepo<int, Direction> Directions { get; }
	IBaseRepo<int, ProdCat1> ProdCats1 { get; }
	IBaseRepo<int, ProdCat2> ProdCats2 { get; }
	IBaseRepo<int, ProdCat3> ProdCats3 { get; }
	IBaseRepo<int, Product> Products { get; }
	IBaseRepo<int, Brand> Brands { get; }
}

public class DALContext : IDALContext
{
	public DALContext(IMongoDbContext mongoDbContext)
	{
		Users = new UsersRepo(mongoDbContext);
		Directions = new DirectionsRepo(mongoDbContext);
		ProdCats1 = new ProdCats1Repo(mongoDbContext);
		ProdCats2 = new ProdCats2Repo(mongoDbContext);
		ProdCats3 = new ProdCats3Repo(mongoDbContext);
		Products = new ProductsRepo(mongoDbContext);
		Brands = new BrandsRepo(mongoDbContext);
	}

	public IBaseRepo<int, User> Users { get; }
	public IBaseRepo<int, Direction> Directions { get; }
	public IBaseRepo<int, ProdCat1> ProdCats1 { get; }
	public IBaseRepo<int, ProdCat2> ProdCats2 { get; }
	public IBaseRepo<int, ProdCat3> ProdCats3 { get; }
	public IBaseRepo<int, Product> Products { get; }
	public IBaseRepo<int, Brand> Brands { get; }
}