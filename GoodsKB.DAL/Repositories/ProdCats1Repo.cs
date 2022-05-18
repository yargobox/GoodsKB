using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Repositories.Mongo;

namespace GoodsKB.DAL.Repositories;

internal class ProdCats1Repo : SoftDelRepoMongo<int?, ProdCat1, DateTimeOffset>
{
	public ProdCats1Repo(IMongoDbContext context)
		: base(context, "product_categories_1", new PesemisticSequentialIdGenerator<ProdCat1, DateTimeOffset>())
	{
	}
}