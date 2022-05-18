namespace GoodsKB.DAL.Repositories;

using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Repositories.Mongo;

internal class DirectionsRepo : SoftDelRepoMongo<int?, Direction, DateTimeOffset>
{
	public DirectionsRepo(IMongoDbContext context)
		: base(context, "directions", new PesemisticSequentialIdGenerator<Direction, DateTimeOffset>())
	{
	}
}