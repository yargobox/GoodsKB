using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Repositories.Mongo;

namespace GoodsKB.DAL.Repositories;

[SequentialIdentity]
internal class BrandsRepo : MongoSoftDelRepo<int, Brand, DateTimeOffset>
{
	public BrandsRepo(IMongoDbContext context)
		: base(context, "brands")
	{
	}
}