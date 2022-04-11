using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;

namespace GoodsKB.DAL.Repositories;

internal class BrandsRepo : MongoIntIdentityRepo<Brand>
{
	public BrandsRepo(IMongoDbContext context)
		: base(context, "brands")
	{
	}
}