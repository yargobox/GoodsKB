using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;

namespace GoodsKB.DAL.Repositories;

internal class DirectionsRepo : MongoSoftDelRepo<int, Direction, DateTimeOffset>
{
	public DirectionsRepo(IMongoDbContext context)
		: base(context, "directions")
	{
	}
}