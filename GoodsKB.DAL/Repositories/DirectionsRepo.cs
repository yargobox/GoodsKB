using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Repositories.Mongo;

namespace GoodsKB.DAL.Repositories;

[SequentialIdentity]
internal class DirectionsRepo : MongoSoftDelRepo<int, Direction, DateTimeOffset>
{
	public DirectionsRepo(IMongoDbContext context)
		: base(context, "directions")
	{
	}
}