using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Repositories.Mongo;

namespace GoodsKB.DAL.Repositories;

internal class ProdCats3Repo : SoftDelRepoMongo<int?, ProdCat3, DateTimeOffset>
{
	public ProdCats3Repo(IMongoDbContext context)
		: base(context, "product_categories_3", new PesemisticSequentialIdGenerator<ProdCat3, DateTimeOffset>())
	{
	}
}