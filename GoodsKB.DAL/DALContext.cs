using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;

using GoodsKB.DAL.Repositories;

namespace GoodsKB.DAL;

public interface IDALContext
{
	IMongoSoftDelRepo<int, User, DateTimeOffset> Users { get; }
	IRepoBase<int, Direction> Directions { get; }
	IRepoBase<int, ProdCat1> ProdCats1 { get; }
	IRepoBase<int, ProdCat2> ProdCats2 { get; }
	IRepoBase<int, ProdCat3> ProdCats3 { get; }
	IRepoBase<int, Product> Products { get; }
	IRepoBase<int, Brand> Brands { get; }
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

	public IMongoSoftDelRepo<int, User, DateTimeOffset> Users { get; }
	public IRepoBase<int, Direction> Directions { get; }
	public IRepoBase<int, ProdCat1> ProdCats1 { get; }
	public IRepoBase<int, ProdCat2> ProdCats2 { get; }
	public IRepoBase<int, ProdCat3> ProdCats3 { get; }
	public IRepoBase<int, Product> Products { get; }
	public IRepoBase<int, Brand> Brands { get; }
}