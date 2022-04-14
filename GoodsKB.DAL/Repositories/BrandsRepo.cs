using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;

namespace GoodsKB.DAL.Repositories;

internal class BrandsRepo : MongoSoftDelRepo<int, Brand, DateTimeOffset>
{
	public BrandsRepo(IMongoDbContext context)
		: base(context, "brands")
	{
	}
}