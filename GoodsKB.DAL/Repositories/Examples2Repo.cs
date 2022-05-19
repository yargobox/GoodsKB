using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Repositories.Mongo;

namespace GoodsKB.DAL.Repositories;

internal class Examples2Repo : SoftDelRepoMongo<Example2Id, Example2, DateTimeOffset>
{
	public Examples2Repo(IMongoDbContext context) : base(context, "examples2")
	{
	}
}