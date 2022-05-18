using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Repositories.Mongo;

namespace GoodsKB.DAL.Repositories;

internal class Examples1Repo : SoftDelRepoMongo<Example1Id, Example1, DateTimeOffset>
{
	public Examples1Repo(IMongoDbContext context) : base(context, "examples1")
	{
	}
}