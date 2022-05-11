using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Repositories.Mongo;

namespace GoodsKB.DAL.Repositories;

internal class ProductsRepo : SoftDelRepoMongo<int, Product, DateTimeOffset>
{
	public ProductsRepo(IMongoDbContext context)
		: base(context, "products",
			new SequenceIdentityProvider<int>(context, "products", 0, 1, x => x == 0)
		)
	{
	}
}