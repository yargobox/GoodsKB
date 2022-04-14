using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;

namespace GoodsKB.DAL.Repositories;

internal class ProdCats1Repo : MongoSoftDelRepo<int, ProdCat1, DateTimeOffset>
{
	public ProdCats1Repo(IMongoDbContext context)
		: base(context, "product_categories_1")
	{
	}
}