using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Repositories.Mongo;
using MongoDB.Driver;

namespace GoodsKB.DAL.Repositories;

[SequentialIdentity]
internal class UsersRepo : MongoSoftDelRepo<int, User, DateTimeOffset>
{
	public UsersRepo(IMongoDbContext context)
		: base(context, "users")
	{
	}

	protected override async void OnRepositoryCheck()
	{
		base.OnRepositoryCheck();

		var indexeNames = await GetIndexNames();

		var indexName = "username_ux";
		if ( !indexeNames.Contains(indexName) ) await CreateIndex(indexName, true, x => x.Username);

		indexName = "email_ux";
		if ( !indexeNames.Contains(indexName) ) await CreateIndex(indexName, true, x => x.Email);

		indexName = "phone_ux";
		if ( !indexeNames.Contains(indexName) ) await CreateIndex(indexName, true, x => x.Phone);
	}
}