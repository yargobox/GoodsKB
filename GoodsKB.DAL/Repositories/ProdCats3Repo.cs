using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Repositories.Mongo;

namespace GoodsKB.DAL.Repositories;

[SequentialIdentity]
internal class ProdCats3Repo : MongoSoftDelRepo<int, ProdCat3, DateTimeOffset>
{
	public ProdCats3Repo(IMongoDbContext context)
		: base(context, "product_categories_3")
	{
	}
}