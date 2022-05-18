using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Repositories.Mongo;

namespace GoodsKB.DAL.Repositories;

internal class BrandsRepo : SoftDelRepoMongo<int?, Brand, DateTimeOffset>
{
	public BrandsRepo(IMongoDbContext context)
		: base(context, "brands", new PesemisticSequentialIdGenerator<Brand, DateTimeOffset>())
	{
	}
}