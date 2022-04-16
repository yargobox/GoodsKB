using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Repositories.Mongo;

namespace GoodsKB.DAL.Repositories;

internal class BrandsRepo : MongoSoftDelRepo<int, Brand, DateTimeOffset>
{
	public BrandsRepo(IMongoDbContext context)
		: base(context, "brands",
			new SequenceIdentityProvider<int>(context, "brands", 0, 1, x => x == 0)
		)
	{
	}
}