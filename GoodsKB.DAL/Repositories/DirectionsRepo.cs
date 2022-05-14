namespace GoodsKB.DAL.Repositories;

using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Repositories.Mongo;

internal class DirectionsRepo : SoftDelRepoMongo<int?, Direction, DateTimeOffset>
{
	public DirectionsRepo(IMongoDbContext context)
		: base(context, "directions",
			new SequenceIdentityProvider<int?>(context, "directions", 0, 1, x => x == 0)
		)
	{
	}
}