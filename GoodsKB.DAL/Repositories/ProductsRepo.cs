using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;

namespace GoodsKB.DAL.Repositories;

internal class ProductsRepo : MongoIntIdentityRepo<Product>
{
	public ProductsRepo(IMongoDbContext context)
		: base(context, "products")
	{
	}
}