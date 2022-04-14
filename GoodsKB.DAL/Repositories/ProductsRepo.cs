using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Repositories.Mongo;

namespace GoodsKB.DAL.Repositories;

[SequentialIdentity]
internal class ProductsRepo : MongoSoftDelRepo<int, Product, DateTimeOffset>
{
	public ProductsRepo(IMongoDbContext context)
		: base(context, "products")
	{
	}
}