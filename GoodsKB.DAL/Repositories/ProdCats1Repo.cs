using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Repositories.Mongo;

namespace GoodsKB.DAL.Repositories;

[SequentialIdentity]
internal class ProdCats1Repo : MongoSoftDelRepo<int, ProdCat1, DateTimeOffset>
{
	public ProdCats1Repo(IMongoDbContext context)
		: base(context, "product_categories_1")
	{
	}
}