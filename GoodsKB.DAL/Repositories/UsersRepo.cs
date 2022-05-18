using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Repositories.Mongo;

namespace GoodsKB.DAL.Repositories;

internal class UsersRepo : SoftDelRepoMongo<int?, User, DateTimeOffset>
{
	public UsersRepo(IMongoDbContext context)
		: base(context, "users", new PesemisticSequentialIdGenerator<User, DateTimeOffset>())
	{
	}

	protected override async void OnLoad()
	{
		base.OnLoad();

		var indexeNames = await GetIndexNames();

		var indexName = "username_ux";
		if (!indexeNames.Contains(indexName))
		{
			await CreateIndexAsync(indexName, true, false, x => x.Username, false, Collations.Ukrainian_CI_AS);
		}

		indexName = "email_ux";
		if (!indexeNames.Contains(indexName))
		{
			await CreateIndexAsync(indexName, true, true, x => x.Email, false, Collations.Ukrainian_CI_AS);
		}

		indexName = "phone_ux";
		if (!indexeNames.Contains(indexName))
		{
			await CreateIndexAsync(indexName, true, true, x => x.Phone, false, Collations.Ukrainian_CI_AS);
		}
	}
}