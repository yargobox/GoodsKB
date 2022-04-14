using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;

namespace GoodsKB.DAL.Repositories;

internal class ProductsRepo : MongoSoftDelRepo<int, Product, DateTimeOffset>
{
	public ProductsRepo(IMongoDbContext context)
		: base(context, "products")
	{
	}
}