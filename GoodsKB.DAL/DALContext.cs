using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Repositories;
using GoodsKB.DAL.Repositories.Mongo;

namespace GoodsKB.DAL;

public interface IDALContext
{
	ISoftDelRepoMongo<int, User, DateTimeOffset> Users { get; }
	ISoftDelRepoMongo<int, Direction, DateTimeOffset> Directions { get; }
	ISoftDelRepoMongo<int, ProdCat1, DateTimeOffset> ProdCats1 { get; }
	ISoftDelRepoMongo<int, ProdCat2, DateTimeOffset> ProdCats2 { get; }
	ISoftDelRepoMongo<int, ProdCat3, DateTimeOffset> ProdCats3 { get; }
	ISoftDelRepoMongo<int, Product, DateTimeOffset> Products { get; }
	ISoftDelRepoMongo<int, Brand, DateTimeOffset> Brands { get; }
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

	public ISoftDelRepoMongo<int, User, DateTimeOffset> Users { get; }
	public ISoftDelRepoMongo<int, Direction, DateTimeOffset> Directions { get; }
	public ISoftDelRepoMongo<int, ProdCat1, DateTimeOffset> ProdCats1 { get; }
	public ISoftDelRepoMongo<int, ProdCat2, DateTimeOffset> ProdCats2 { get; }
	public ISoftDelRepoMongo<int, ProdCat3, DateTimeOffset> ProdCats3 { get; }
	public ISoftDelRepoMongo<int, Product, DateTimeOffset> Products { get; }
	public ISoftDelRepoMongo<int, Brand, DateTimeOffset> Brands { get; }
}