using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Repositories;
using GoodsKB.DAL.Repositories.Mongo;

namespace GoodsKB.DAL;

public interface IDALContext
{
	IMongoSoftDelRepo<int, User, DateTimeOffset> Users { get; }
	IMongoSoftDelRepo<int, Direction, DateTimeOffset> Directions { get; }
	IMongoSoftDelRepo<int, ProdCat1, DateTimeOffset> ProdCats1 { get; }
	IMongoSoftDelRepo<int, ProdCat2, DateTimeOffset> ProdCats2 { get; }
	IMongoSoftDelRepo<int, ProdCat3, DateTimeOffset> ProdCats3 { get; }
	IMongoSoftDelRepo<int, Product, DateTimeOffset> Products { get; }
	IMongoSoftDelRepo<int, Brand, DateTimeOffset> Brands { get; }
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
	public IMongoSoftDelRepo<int, Direction, DateTimeOffset> Directions { get; }
	public IMongoSoftDelRepo<int, ProdCat1, DateTimeOffset> ProdCats1 { get; }
	public IMongoSoftDelRepo<int, ProdCat2, DateTimeOffset> ProdCats2 { get; }
	public IMongoSoftDelRepo<int, ProdCat3, DateTimeOffset> ProdCats3 { get; }
	public IMongoSoftDelRepo<int, Product, DateTimeOffset> Products { get; }
	public IMongoSoftDelRepo<int, Brand, DateTimeOffset> Brands { get; }
}