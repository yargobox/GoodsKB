using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;

namespace GoodsKB.DAL.Repositories;

internal class DirectionsRepo : MongoIntIdentityRepo<Direction>
{
	public DirectionsRepo(IMongoDbContext context)
		: base(context, "directions")
	{
	}
}