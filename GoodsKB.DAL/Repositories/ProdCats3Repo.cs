using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;

namespace GoodsKB.DAL.Repositories;

internal class ProdCats3Repo : MongoSoftDelRepo<int, ProdCat3, DateTimeOffset>
{
	public ProdCats3Repo(IMongoDbContext context)
		: base(context, "product_categories_3")
	{
	}
}