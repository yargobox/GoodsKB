using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Repositories.Mongo;

namespace GoodsKB.DAL.Repositories;

internal class ProdCats2Repo : SoftDelRepoMongo<int?, ProdCat2, DateTimeOffset>
{
	public ProdCats2Repo(IMongoDbContext context)
		: base(context, "product_categories_2",
			new SequenceIdentityProvider<int?>(context, "product_categories_2", 0, 1, x => x == 0)
		)
	{
	}
}