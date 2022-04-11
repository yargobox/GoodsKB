using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;

namespace GoodsKB.DAL.Repositories;

internal class ProdCats2Repo : MongoIntIdentityRepo<ProdCat2>
{
	public ProdCats2Repo(IMongoDbContext context)
		: base(context, "product_categories_2")
	{
	}
}